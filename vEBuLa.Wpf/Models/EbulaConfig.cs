using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace vEBuLa.Models;
public class EbulaConfig {
  private readonly ILogger? Logger = App.AppHost?.Services.GetRequiredService<ILogger<EbulaConfig>>();

  public override string ToString() => $"{Name}: {Stations.Count}/{Segments.Count}";
  public string Name { get; private set; } = string.Empty;
  public Dictionary<Guid, EbulaStation> Stations { get; private set; } = new();
  public Dictionary<Guid, EbulaSegment> Segments { get; private set; } = new();

  public EbulaConfig() { }

  public EbulaConfig(string jsonData) {
    var jConfig = JObject.Parse(jsonData);
    Name = jConfig.Value<string>(nameof(Name)) ?? "noname";
    var jStations = jConfig.Value<JArray>("Stations");
    if (jStations is null) {
      Logger?.LogError("Config {Config} contains no Station entries", this);
      return;
    }
    var jSegments = jConfig.Value<JArray>("Segments");
    if (jSegments is null) {
      Logger?.LogError("Config {Config} contains no Segment entries", this);
      return;
    }

    foreach (var jStation in jStations) {
      if (jStation is not JObject || jStation.ToObject<EbulaStation>() is not EbulaStation station) {
        Logger?.LogWarning("Station {StationJson} failed to parse", jStation);
        continue;
      }
      if (Stations.TryGetValue(station.Id, out var st)) {
        Logger?.LogWarning("Station {Station} shares ID {StationId} with {ExistingStation}", station, station.Id, st);
        continue;
      }
      Logger?.LogDebug("Adding Station {Station} to config", station);
      Stations[station.Id] = station;
    }

    foreach (var jSegment in jSegments) {
      if (jSegment is not JObject) {
        Logger?.LogWarning("Segment {SegmentJson} failed to parse", jSegment);
        continue;
      }
      var segment = new EbulaSegment((JObject) jSegment);
      if (Segments.TryGetValue(segment.Id, out var seg)) {
        Logger?.LogWarning("Segment {Segment} shares ID {SegmentId} with {ExistingSegment}", segment, segment.Id, seg);
        continue;
      }
      Logger?.LogDebug("Adding Segment {Segment} to config", segment);
      Segments[segment.Id] = segment;
    }
  }
}
