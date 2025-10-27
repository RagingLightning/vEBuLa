using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Documents;
using vEBuLa.Commands;
using vEBuLa.Extensions;
using vEBuLa.ViewModels;

namespace vEBuLa.Models;

public class EbulaService : IEbulaService {
  private ILogger<EbulaConfig>? Logger => App.AppHost?.Services.GetRequiredService<ILogger<EbulaConfig>>();
  public override string ToString() => $"{Name.Crop(6)} @ {StartTime:hh\\:mm} - {Route.Id.ToString().BiCrop(5,5)}";

  public EbulaService(EbulaConfig config, Guid id, JObject jService) {
    Id = id;
    Config = config;
    Name = jService.Value<string>(nameof(Name)) ?? "000000";
    Description = jService.Value<string>(nameof(Description)) ?? "000000";

    if (TimeSpan.TryParse(jService.Value<string>(nameof(StartTime)), out var serviceStart)) {
      StartTime = serviceStart;
    }
    else
      Logger?.LogWarning("Failed to parse service start {ServiceStart}", jService.Value<string>(nameof(StartTime)));

    var rawRouteId = jService.Value<string>(nameof(Route));
    if (ParseRoute(rawRouteId) is EbulaRoute route) {
      Route = route;
    }
    else {
      throw new MissingMemberException($"No Route with Id {rawRouteId} found for Service {this}!");
    }

      Stops = [];
    if (jService.Value<JArray>(nameof(Stops)) is JArray jStops)
    foreach (var jStop in jStops) {
      if (jStop.ToObject<EbulaServiceStop>() is not EbulaServiceStop stop) {
        Logger?.LogWarning("Stop {StopJson} failed to parse", jStop);
        continue;
      }
      Logger?.LogTrace("Stop {Stop} parsed successfully", stop);
      Stops.Add(stop);
    }

    Vehicles = new();
    if (jService.Value<JArray>(nameof(Vehicles)) is JArray jVehicles)
      foreach (var jVehicle in jVehicles)
        if (jVehicle.ToObject<string>() is string vehicle)
          Vehicles.Add(vehicle);
    //Compatibility
    if (jService.Value<string>("Vehicle") is string v)
      Vehicles.Add(v);
  }

  public EbulaService(EbulaConfig config, Guid id, EbulaRoute selectedRoute, string serviceName, TimeSpan startTime, string description, List<string> vehicles) {
    Id = id;
    Config = config;
    Route = selectedRoute;
    Name = serviceName;
    StartTime = startTime;
    Description = description;
    Vehicles = vehicles;
    Stops = [];
  }

  public EbulaServiceVM ToVM(BaseC? editCommand = null, SetupScreenVM? screen = null) {
    return new EbulaServiceVM(this, editCommand, screen);
  }

  private EbulaRoute? ParseRoute(string? rawRouteId) {
    if (string.IsNullOrEmpty(rawRouteId)) {
      return null;
    }

    if (rawRouteId.Contains('.')) { // ConfigId.StationId format
      var parts = rawRouteId.Split('.', 2);
      var configId = Guid.Parse(parts[0]);
      var routeId = Guid.Parse(parts[1]);

      EbulaConfig? routeConfig;
      if (Config.Id == configId) {
        routeConfig = Config;
      }
      else if (!Config.ResolvedDependencies.TryGetValue(configId, out routeConfig)) {
        Logger?.LogWarning("Route {RouteId} references missing config {ConfigId}", routeId, configId);
        return null;
      }

      if (!routeConfig.Routes.TryGetValue(routeId, out var route)) {
        Logger?.LogWarning("No Route with ID {RouteId} found in config {ConfigId}!", routeId, configId);
        return null;
      }

      return route;
    }
    else { // Deprecated: Station from same config without config prefix
      if (!Guid.TryParse(rawRouteId, out var routeId)) {
        Logger?.LogWarning("Route ID {RouteIdJson} failed to parse", rawRouteId);
        return null;
      }
      if (!Config.Routes.TryGetValue(routeId, out var route)) {
        Logger?.LogWarning("No Station with Id {StationId} found!", rawRouteId);
        return null;
      }
      return route;
    }
  }

  internal void ShiftStops(IEnumerable<EbulaServiceStop> stops, TimeSpan fromDeparture, TimeSpan toDeparture) {
    Stops.AddRange(stops.Select(s => s.Copy()));
    foreach (var stop in Stops) {
      if (stop.Arrival is DateTime a)
        stop.Arrival = a + toDeparture - fromDeparture;
      if (stop.Departure is DateTime d)
        stop.Departure = d + toDeparture - fromDeparture;
    }
  }

  [JsonIgnore] public EbulaConfig Config { get; }
  [JsonIgnore] public Guid Id { get; set; } = Guid.Empty;
  public string Name { get; set; } = string.Empty;
  [JsonIgnore] public string Number => String.Concat(Name.Where(char.IsDigit));
  [JsonProperty(nameof(Route))] private string RouteId => $"{Route.Config.Id}.{Route.Id}";
  [JsonIgnore] public EbulaRoute Route { get; set; }
  [JsonIgnore] public List<EbulaSegment> Segments => Route.Segments;
  [JsonProperty(nameof(Stops))] private IEnumerable<EbulaServiceStop> FilteredStops => Stops.Where(s => !s.IsEmpty());
  [JsonIgnore] public List<EbulaServiceStop> Stops { get; set; }
  public TimeSpan StartTime { get; set; }
  public string Description { get; set; } = string.Empty;
  public List<string> Vehicles { get; set; }
}