using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Security.Cryptography.Xml;
using System.Threading;
using System.Threading.Tasks;
using vEBuLa.Events;
using vEBuLa.Extensions;

namespace vEBuLa.Models;

public interface IEbulaGameApi : IDisposable {
  bool IsAvailable { get; }

  Vector2? GetPosition();
  void MonitorPositions(IEnumerable<EbulaEntry> positions);
  event PositionPassedEventHandler? PositionPassed;

  DateTime? GetGameTime();
}

public class EbulaTswApi : IEbulaGameApi {
  private static readonly string ENDPOINT_PLAYER_DATA = "DriverAid.PlayerInfo";
  private static readonly string ENDPOINT_TIME_DATA = "TimeOfDay.Data";

  private ILogger? Logger => App.GetService<ILogger<EbulaTswApi>>();
  public event PositionPassedEventHandler? PositionPassed;

  public EbulaTswApi(string apiKey) {
    _httpClient.DefaultRequestHeaders.Add("User-Agent", "vEBuLa");
    _httpClient.DefaultRequestHeaders.Accept.Clear();
    _httpClient.DefaultRequestHeaders.Accept.Add(new("application/json"));
    _httpClient.DefaultRequestHeaders.Add("DTGCommKey", apiKey);

    Task.Run(MessageLoop, _messageLoopCancel.Token);
  }

  public void Dispose() {
    _messageLoopCancel.Cancel();
  }

  public DateTime? GetGameTime() {
    return _lastGameTime;
  }

  public Vector2? GetPosition() {
    return _lastPosition;
  }

  public void MonitorPositions(IEnumerable<EbulaEntry> entries) {
    _monitoringList.Clear();
    _monitoringList.AddRange(entries.Where(e => e.GpsLocation is not null));
    Logger?.LogDebug("Monitoring {count} positions", _monitoringList.Count);
  }

  private void CheckPosition(JObject positionData) {
    Vector2 position;
    if (positionData["geoLocation"] is not JObject geoData
      || geoData["latitude"] is not JToken latToken
      || geoData["longitude"] is not JToken lngToken) {
      Logger?.LogWarning("Invalid position data: {data}", positionData);
      return;
    }
    else {
      position = new Vector2(latToken.Value<float>(), lngToken.Value<float>());
    }

    if (_monitoringList.Count == 0
      || _lastPosition is not Vector2 lastPosition) {
      _lastPosition = position;
      return;
    }

    EbulaEntry? targetEntry = null;
    var orderedList = _monitoringList.OrderBy(e => Vector2.Distance(position, e.GpsLocation ?? Vector2.Zero));
    foreach (var entry in orderedList) {
      if (entry.GpsLocation is not null) {
        targetEntry = entry;
        break;
      }
    }

    if (targetEntry is null || targetEntry.GpsLocation is not Vector2 targetPos) {
      Logger?.LogWarning("No target entry to compare against!");
      return;
    }

    var distance = Vector2.Distance(position, targetPos);
    var passingScore = MathEx.SymmtricLinePointPassingScore(lastPosition, position, targetPos);
    Logger?.LogTrace("Checking {position} for {Entry}", position, targetEntry);
    Logger?.LogTrace("Entry Position: {Position}", targetPos);
    Logger?.LogTrace("Passing Score: {Score}, Distance: {Distance}", passingScore, distance);

    if (Math.Abs(passingScore) < 1.1 && distance < 0.001) {
      Logger?.LogDebug("{Entry} passed at {Position} (Score of {Score})", targetEntry, position, passingScore);
      PositionPassed?.Invoke(this, new(targetEntry, position));
    }
    else if (distance < 0.00005) {
      Logger?.LogDebug("{Entry} reached at {Position} (Distance of {Distance})", targetEntry, position, distance);
      PositionPassed?.Invoke(this, new(targetEntry, position));
    }

    _lastPosition = position;
  }

  private void CheckTime(JObject timeData) {
    if (timeData["LocalTimeISO8601"] is not JToken localTimeToken) {
      Logger?.LogWarning("Invalid time data: {data}", timeData);
      Logger?.LogDebug("Data is missing required key: {key}", "LocalTimeISO8601");
      return;
    }

    var localTime = localTimeToken.Value<DateTime>();
    var localSun = localTime.IsDaylightSavingTime();
    var newTime = new DateTime(localTime.Year, localTime.Month, localTime.Day, localSun ? localTime.Hour - 1 : localTime.Hour, localTime.Minute, localTime.Second);
    _lastGameTime = newTime;
    Logger?.LogTrace("Game Time is: {gameTime}", newTime);
  }

  private async Task MessageLoop() {
    while (!_messageLoopCancel.IsCancellationRequested) {
      try {
        var nowAvail = IsAvailable;

        if (_setupTimeout >= 30
          && await SetupSubscriptions()) {
          _setupTimeout = 0;
          Logger?.LogDebug("TSW API setup completed");
        }

        nowAvail = await CheckSubscriptions();

        if (IsAvailable != nowAvail) {
          if (_setupTimeout > 5)
            Logger?.LogDebug("TSW API availability: {nowAvail}", nowAvail);
          IsAvailable = nowAvail;
        }
      }
      catch (HttpRequestException e) {
        IsAvailable = false;
        _setupTimeout += 1;
        if (_setupTimeout > 5)
          Logger?.LogWarning("TSW API error: {error}", e.Message);
      }
      catch (Exception e) {
        Logger?.LogWarning("TSW API error: {error}", e.Message);
      }
      await Task.Delay(1000); // Poll every second
    }
  }

  private async Task<bool> SetupSubscriptions() {
    // Delete subscriptions if any exist
    if (!await DEL_Subscription(8192)) return false;

    // Create new subscriptions
    if (!await POST_Subscription(8192, ENDPOINT_PLAYER_DATA)) return false;
    if (!await POST_Subscription(8192, ENDPOINT_TIME_DATA)) return false;

    return true;
  }

  private async Task<bool> CheckSubscriptions() {
    var subData = await GET_Subscription(8192);
    if (subData is not JObject data) return false;

    var entries = data["Entries"] as JArray;
    var positionEntry = entries?.FirstOrDefault(e => e["Path"]?.ToString() == ENDPOINT_PLAYER_DATA);
    var timeEntry = entries?.FirstOrDefault(e => e["Path"]?.ToString() == ENDPOINT_TIME_DATA);

    // Process position data
    var positionData = positionEntry?["Values"] as JObject;
    if (positionData is not null)
      CheckPosition(positionData);

    // Process game time data
    var timeData = timeEntry?["Values"] as JObject;
    if (timeData is not null)
      CheckTime(timeData);

    return true;
  }

  private async Task<bool> GET_Info() {
    var res = await _httpClient.GetAsync("http://127.0.0.1:31270/info");
    return res.IsSuccessStatusCode;
  }

  private async Task<bool> POST_Subscription(uint id, string endpoint) {
    var res = await _httpClient.PostAsync($"http://127.0.0.1:31270/subscription/{endpoint}?Subscription={id}", null);
    return res.IsSuccessStatusCode;
  }

  private async Task<bool> DEL_Subscription(uint id) {
    var res = await _httpClient.DeleteAsync($"http://127.0.0.1:31270/subscription?Subscription={id}");
    return res.IsSuccessStatusCode || res.StatusCode == HttpStatusCode.BadRequest;
  }

  private async Task<JObject?> GET_Subscription(uint id) {
    var res = await _httpClient.GetAsync($"http://127.0.0.1:31270/subscription?Subscription={id}");
    if (!res.IsSuccessStatusCode) return default;
    var json = await res.Content.ReadAsStringAsync();
    try { return JObject.Parse(json); }
    catch (JsonException) { return null; }
  }

  private readonly HttpClient _httpClient = new();
  private readonly CancellationTokenSource _messageLoopCancel = new();
  private int _setupTimeout = int.MaxValue;
  private readonly List<EbulaEntry> _monitoringList = [];
  private Vector2? _lastPosition;
  private DateTime? _lastGameTime;

  public bool IsAvailable { get; private set; }
}