using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using vEBuLa.Extensions;
using vEBuLa.ViewModels;

namespace vEBuLa.Models;
internal class EbulaConfig {
  private ILogger<EbulaConfig>? Logger => App.AppHost?.Services.GetRequiredService<ILogger<EbulaConfig>>();

  public override string ToString() => $"{Name.Crop(30)} ({Stations.Count}/{Segments.Count}/{Routes.Count})";
  public string Name { get; set; } = "Unnamed Configuration";
  public Dictionary<Guid, EbulaStation> Stations { get; private set; } = new();
  public Dictionary<Guid, EbulaSegment> Segments { get; private set; } = new();
  public Dictionary<Guid, EbulaRoute> Routes { get; private set; } = new();

  public EbulaConfig() { Logger?.LogInformation("Using blank EBuLa Config"); }

  public EbulaConfig(string fileName) {
    Logger?.LogInformation("Loading EBuLa Config from {ConfigFile}", fileName.BiCrop(10,30));
    var jConfig = JObject.Parse(File.ReadAllText(fileName));
    Name = jConfig.Value<string>(nameof(Name)) ?? "Unnamed Config";
    var jStations = jConfig.Value<JObject>(nameof(Stations));
    if (jStations is null) {
      Logger?.LogError("Config {Config} contains no Station entries", this);
      return;
    }
    var jSegments = jConfig.Value<JObject>(nameof(Segments));
    if (jSegments is null) {
      Logger?.LogError("Config {Config} contains no Segment entries", this);
      return;
    }
    var jRoutes = jConfig.Value<JObject>(nameof(Routes));
    if (jRoutes is null) {
      Logger?.LogWarning("Config {Config} contains no Route entries", this);
      jRoutes = new JObject();
    }

    foreach (var jStation in jStations) {
      if (jStation.Value is not JObject) {
        Logger?.LogWarning("Station {StationJson} failed to parse", jStation);
        continue;
      }
      var station = new EbulaStation(Guid.Parse(jStation.Key), (JObject) jStation.Value);
      if (Stations.TryGetValue(station.Id, out var st)) {
        Logger?.LogWarning("Station {Station} shares ID {StationId} with {ExistingStation}", station, station.Id, st);
        continue;
      }
      Logger?.LogDebug("Adding Station {Station} to config", station);
      Stations[station.Id] = station;
    }

    foreach (var jSegment in jSegments) {
      if (jSegment.Value is not JObject) {
        Logger?.LogWarning("Segment {SegmentJson} failed to parse", jSegment);
        continue;
      }
      var segment = new EbulaSegment(this, Guid.Parse(jSegment.Key), (JObject) jSegment.Value);
      if (Segments.TryGetValue(segment.Id, out var seg)) {
        Logger?.LogWarning("Segment {Segment} shares ID {SegmentId} with {ExistingSegment}", segment, segment.Id, seg);
        continue;
      }
      Logger?.LogDebug("Adding Segment {Segment} to config", segment);
      Segments[segment.Id] = segment;
    }

    foreach (var jRoute in jRoutes) {
      if (jRoute.Value is not JObject) {
        Logger?.LogWarning("Route {RouteJson} failed to parse", jRoute);
        continue;
      }
      var route = new EbulaRoute(this, Guid.Parse(jRoute.Key), (JObject) jRoute.Value);
      if (Segments.TryGetValue(route.Id, out var rte)) {
        Logger?.LogWarning("Route {Route} shares ID {RouteId} with {ExistingRoute}", route, route.Id, rte);
        continue;
      }
      Logger?.LogDebug("Adding Route {Route} to config", route);
      Routes[route.Id] = route;
    }

    Logger?.LogInformation("EBuLa Config {Config} fully loaded", this);
  }

  public IEnumerable<EbulaSegment> FindSegments(EbulaStation origin) {
    return Segments.Values.Where(s => s.Origin.Station == origin);
  }

  public EbulaStation AddStation(string name) {
    var id = Guid.NewGuid();
    while (Stations.ContainsKey(id)) id = Guid.NewGuid();
    var station = new EbulaStation(id, name);
    Stations.Add(id, station);
    return station;
  }

  public EbulaSegment AddSegment(string name, EbulaStation? origin) {
    var id = Guid.NewGuid();
    while (Segments.ContainsKey(id)) id = Guid.NewGuid();
    var segment = new EbulaSegment(this, id, name);
    if (origin is not null) segment.Origin = (origin.Id, origin);
    Segments.Add(id, segment);
    return segment;
  }

  public EbulaRoute AddRoute(IEnumerable<EbulaSegment> segments, string routeName, string description, int stations, TimeSpan duration, string routeDesc) {
    var id = Guid.NewGuid();
    while (Routes.ContainsKey(id)) id = Guid.NewGuid();
    var route = new EbulaRoute(id, segments, routeName, description, stations, duration, routeDesc);
    Routes.Add(id, route);
    return route;
  }

  internal void Save(string fileName) {
    JObject jConfig = new JObject();
    jConfig[nameof(Name)] = Name;

    var jStations = new JObject();
    foreach (var station in Stations) jStations[station.Key.ToString()] = JObject.FromObject(station.Value);
    jConfig[nameof(Stations)] = jStations;

    var jSegments = new JObject();
    foreach (var segment in Segments) jSegments[segment.Key.ToString()] = JObject.FromObject(segment.Value);
    jConfig[nameof(Segments)] = jSegments;

    var jRoutes = new JObject();
    foreach (var route in Routes) jRoutes[route.Key.ToString()] = JObject.FromObject(route.Value);
    jConfig[nameof(Routes)] = jRoutes;

    var jsonString = JsonConvert.SerializeObject(jConfig, Formatting.Indented);
    File.WriteAllText(fileName, jsonString);
  }
}
