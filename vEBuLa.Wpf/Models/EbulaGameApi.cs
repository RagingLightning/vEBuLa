using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.Xml;
using System.Threading;
using System.Threading.Tasks;
using vEBuLa.Events;
using vEBuLa.Extensions;

namespace vEBuLa.Models;

public interface IEbulaGameApi : IDisposable {
  bool IsAvailable { get; }

  Position? GetPosition();
  void MonitorPositions(IEnumerable<Position> positions);
  event PositionPassedEventHandler? PositionPassed;

  DateTime? GetGameTime();
}

internal class EbulaTswApi : IEbulaGameApi {
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

  public Position? GetPosition() {
    return _lastPosition;
  }

  public void MonitorPositions(IEnumerable<Position> positions) {
    _monitoringQueue = new Queue<Position>(positions);
    Logger?.LogDebug("Monitoring {count} positions", _monitoringQueue.Count);
  }

  private void CheckPosition(JObject positionData) {
    Position position;
    if (positionData["geoLocation"] is not JObject geoData
      || geoData["latitude"] is not JToken latToken
      || geoData["longitude"] is not JToken lngToken) {
      Logger?.LogWarning("Invalid position data: {data}", positionData);
      return;
    } else {
      position = new Position(latToken.Value<double>(), lngToken.Value<double>());
    }

    Logger?.LogTrace("Checking {position}", position);
    if (_monitoringQueue is not Queue<Position> queue || queue.Count == 0
      || _lastPosition is not Position lastPosition) {
      _lastPosition = position;
      return;
    }

    var targetPos = _monitoringQueue.Peek();
    var distance = MathEx.Distance(position, targetPos);
    var lastDistance = MathEx.Distance(lastPosition, targetPos);
    Logger?.LogTrace("Target {target} distance was {oldDistance}, is now {newDistance}", targetPos, lastDistance, distance);

    if (lastDistance < distance && distance < 100) {
      Logger?.LogDebug("Position {target} passed", targetPos);
      PositionPassed?.Invoke(this, new(targetPos));
      _monitoringQueue.Dequeue();
    }

    _lastPosition = position;
  }

  private void CheckTime(JObject timeData) {

    if (timeData["LocalTimeISO8601"] is not JToken localTimeToken) {
      Logger?.LogWarning("Invalid time data: {data}", timeData);
      Logger?.LogDebug("Data is missing required key: {key}", "LocalTimeISO8601");
      return;
    }

    var time = localTimeToken.Value<DateTime>();
     _lastGameTime = time;
    Logger?.LogTrace("Game Time is: {gameTime}", time);
  }

  private async Task MessageLoop() {
    while (!_messageLoopCancel.IsCancellationRequested) {
      try {
        var nowAvail = IsAvailable;

        if (!_isSetUp) {
          _isSetUp = await SetupSubscriptions();
          if (_isSetUp)
            Logger?.LogDebug("TSW API setup completed");
        }

        if (_isSetUp) {
          nowAvail = await CheckSubscriptions();
        }

        if (IsAvailable != nowAvail) {
          Logger?.LogDebug("TSW API availability: {nowAvail}", nowAvail);
          IsAvailable = nowAvail;
        }
      }
      catch (HttpRequestException e) {
        IsAvailable = false;
        _isSetUp = false;
        Logger?.LogWarning(e, "TSW API error: {error}", e.Message);
        await Task.Delay(9000);
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
  private bool _isSetUp = false;
  private Queue<Position>? _monitoringQueue;
  private Position? _lastPosition;
  private DateTime? _lastGameTime;

  public bool IsAvailable { get; private set; }
}