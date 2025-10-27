using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using vEBuLa.Extensions;

namespace vEBuLa.Models;

public class EbulaSegment {
  private readonly ILogger<EbulaSegment>? Logger;
  public override string ToString() => $"{Id.ToString().BiCrop(5,5)}: {Origin?.Name.Crop(30)} > {Destination?.Name.Crop(30)}";

  public EbulaSegment(EbulaConfig existingConfig, Guid id, JObject jSegment) {
    Logger = App.GetService<ILogger<EbulaSegment>>();
    Config = existingConfig;

    Id = id;

    var rawOriginId = jSegment.Value<string>(nameof(Origin));
    Origin = ParseStation(rawOriginId);
    if (Origin is null)
      Logger?.LogWarning("No Origin Station with Id {StationId} found!", rawOriginId);

    var rawDestinationId = jSegment.Value<string>(nameof(Destination));
    Destination = ParseStation(rawDestinationId);
    if (Destination is null)
      Logger?.LogWarning("No Destination Station with Id {StationId} found!", rawDestinationId);

    if (TimeSpan.TryParse(jSegment.Value<string>(nameof(Duration)), out var duration)) {
      Duration = duration;
    }
    else
      Logger?.LogWarning("Failed to parse segment duration {Duration}", jSegment.Value<string>(nameof(Duration)));
    Name = jSegment.Value<string>(nameof(Name)) ?? string.Empty;

    if (jSegment.Value<JArray>(nameof(PreEntries)) is JArray jPreEntries) {
      Logger?.LogDebug("Parsing  PreEntries for {Segment}", this);
      ParseEntries(PreEntries, jPreEntries);
    }
    else
      Logger?.LogWarning("Segment {Segment} contains no PreEntries", this);

    if (jSegment.Value<JArray>(nameof(Entries)) is JArray jEntries) {
      Logger?.LogDebug("Parsing     Entries for {Segment}", this);
      ParseEntries(Entries, jEntries);
    }
    else
      Logger?.LogWarning("Segment {Segment} contains no Entries", this);

    if (jSegment.Value<JArray>(nameof(PostEntries)) is JArray jPostEntries) {
      Logger?.LogDebug("Parsing PostEntries for {Segment}", this);
      ParseEntries(PostEntries, jPostEntries);
    }
    else
      Logger?.LogWarning("Segment {Segment} contains no PostEntries", this);


  }

  public EbulaSegment(EbulaConfig existingConfig, Guid id, string name) {
    Config = existingConfig;
    Id = id;
    Name = name;
  }

  private EbulaStation? ParseStation(string? rawStationId) {
    if (string.IsNullOrEmpty(rawStationId)) {
      return null;
    }

    if (rawStationId.Contains('.')) { // ConfigId.StationId format
      var parts = rawStationId.Split('.', 2);
      var configId = Guid.Parse(parts[0]);
      var stationId = Guid.Parse(parts[1]);

      EbulaConfig? stationConfig;
      if (Config.Id == configId) {
        stationConfig = Config;
      }
      else if (!Config.ResolvedDependencies.TryGetValue(configId, out stationConfig)) {
        Logger?.LogWarning("Station ID {StationId} references missing config {ConfigId}", stationId, configId);
        return null;
      }

      if (!stationConfig.Stations.TryGetValue(stationId, out var station)) {
        Logger?.LogWarning("No Station with ID {StationId} found in config {ConfigId}!", stationId, configId);
        return null;
      }

      return station;
    }
    else { // Deprecated: Station from same config without config prefix
      if (!Guid.TryParse(rawStationId, out var stationId)) {
        Logger?.LogWarning("Station ID {StationIdJson} failed to parse", rawStationId);
        return null;
      }
      if (!Config.Stations.TryGetValue(stationId, out var station)) {
        Logger?.LogWarning("No Station with Id {StationId} found!", rawStationId);
        return null;
      }
      return station;
    }
  }

  private void ParseEntries(List<EbulaEntry> list, JArray jArray) {
    foreach (var jEntry in jArray) {
      if (jEntry.ToObject<EbulaEntry>() is not EbulaEntry entry) {
        Logger?.LogWarning("Entry {EntryJson} failed to parse", jEntry);
        continue;
      }
      Logger?.LogTrace("Entry {Entry} parsed successfully", entry);
      list.Add(entry);
    }
  }

  public (List<EbulaEntry> List, int Index)? FindEntry(EbulaEntry entry) {
    if (Entries.Contains(entry))
      return (Entries, Entries.IndexOf(entry));
    if (PreEntries.Contains(entry))
      return (PreEntries, PreEntries.IndexOf(entry));
    if (PostEntries.Contains(entry))
      return (PostEntries, PostEntries.IndexOf(entry));
    return null;
  }

  [JsonIgnore] public EbulaConfig Config { get; }
  [JsonIgnore] public Guid Id { get; } = Guid.Empty;
  public string Name { get; set; } = string.Empty;
  [JsonProperty(nameof(Origin))] private string OriginId => $"{Origin?.Config.Id ?? Guid.Empty}.{Origin?.Id ?? Guid.Empty}";
  [JsonIgnore] public EbulaStation? Origin { get; set; } = null;
  [JsonProperty(nameof(Destination))] private string DestinationId => $"{Destination?.Config.Id ?? Guid.Empty}.{Destination?.Id ?? Guid.Empty}";
  [JsonIgnore] public EbulaStation? Destination { get; set; } = null;
  public TimeSpan Duration { get; set; } = new TimeSpan(0, 0, 0);
  public List<EbulaEntry> PreEntries { get; } = new();
  public List<EbulaEntry> Entries { get; } = new();
  public List<EbulaEntry> PostEntries { get; } = new();
}
