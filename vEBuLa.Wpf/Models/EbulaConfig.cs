using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using vEBuLa.Extensions;
using vEBuLa.ViewModels;

namespace vEBuLa.Models;

public class EbulaConfig {
  private ILogger<EbulaConfig>? Logger => App.AppHost?.Services.GetRequiredService<ILogger<EbulaConfig>>();
  public override string ToString() => $"{Name.Crop(30)} ({Stations.Count}/{Segments.Count}/{Routes.Count})";

  public bool IsEmpty { get; } = false;
  public void MarkDirty() { IsDirty = true; }
  public bool IsDirty { get; private set; } = false;
  public string Name { get; set; } = "Unnamed Configuration";
  public Guid Id { get; } = Guid.NewGuid();
  public Dictionary<Guid, string> Dependencies { get; private set; } = new();
  public Dictionary<Guid, EbulaConfig> ResolvedDependencies { get; private set; } = [];
  public bool Available => Dependencies.Keys.All(ResolvedDependencies.ContainsKey);
  public bool Initialized { get; private set; } = false;

  public Dictionary<Guid, EbulaStation> Stations { get; private set; } = new();
  public Dictionary<Guid, EbulaSegment> Segments { get; private set; } = new();
  public Dictionary<Guid, EbulaRoute> Routes { get; private set; } = new();
  public Dictionary<Guid, EbulaService> Services { get; private set; } = new();
  public Dictionary<string, int> Vehicles { get; private set; } = new();

  private string? _originFileName;
  private JObject? _jConfig;

  public EbulaConfig() {
    Logger?.LogInformation("Using blank EBuLa Config");
    IsEmpty = true;
  }

  public EbulaConfig(string fileName) {
    Logger?.LogInformation("Loading EBuLa Config from {ConfigFile}", fileName.BiCrop(10, 30));
    var jConfig = JObject.Parse(File.ReadAllText(fileName));

    if (jConfig.Value<string>(nameof(Id)) is string idString)
      Id = Guid.Parse(idString);
    else
      Id = Guid.NewGuid();

    Name = jConfig.Value<string>(nameof(Name)) ?? "Unnamed Config";

    var jDependencies = jConfig.Value<JObject>(nameof(Dependencies));
    if (jDependencies is not null) {
      foreach (var jDependency in jDependencies)
        Dependencies[Guid.Parse(jDependency.Key)] = jDependency.Value!.ToString();
    }

    _originFileName = fileName;
    _jConfig = jConfig;

    if (Dependencies.Count == 0)
      Initialize();
  }

  public void Initialize() {
    if (Initialized || IsEmpty)
      return; // Already initialized

    if (!Available)
      throw new Exception($"Config {this} has unresolved dependencies and cannot be initialized");

    if (_jConfig is null)
      throw new Exception($"Config {this} has no JSON data to initialize from");

    Logger?.LogDebug("Initializing {config}", this);

    foreach (var dependency in ResolvedDependencies.Where(d => !d.Value.Initialized)) {
      dependency.Value.Initialize();
    }

    var jStations = _jConfig.Value<JObject>(nameof(Stations));
    if (jStations is null) {
      Logger?.LogError("Config {Config} contains no Station entries", this);
      return;
    }
    var jSegments = _jConfig.Value<JObject>(nameof(Segments));
    if (jSegments is null) {
      Logger?.LogError("Config {Config} contains no Segment entries", this);
      return;
    }
    var jRoutes = _jConfig.Value<JObject>(nameof(Routes));
    if (jRoutes is null) {
      Logger?.LogWarning("Config {Config} contains no Route entries", this);
      jRoutes = new JObject();
    }
    var jServices = _jConfig.Value<JObject>(nameof(Services));
    if (jServices is null) {
      Logger?.LogWarning("Config {Config} contains no Service entries", this);
      jServices = new JObject();
    }

    foreach (var jStation in jStations) {
      if (jStation.Value is not JObject) {
        Logger?.LogWarning("Station {StationJson} failed to parse", jStation);
        continue;
      }
      var station = new EbulaStation(this, Guid.Parse(jStation.Key), (JObject) jStation.Value);
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

    foreach (var jService in jServices) {
      if (jService.Value is not JObject) {
        Logger?.LogWarning("Service {ServiceJson} failed to parse", jService);
        continue;
      }
      var service = new EbulaService(this, Guid.Parse(jService.Key), (JObject) jService.Value);
      if (Segments.TryGetValue(service.Id, out var svc)) {
        Logger?.LogWarning("Service {Service} shares ID {ServiceId} with {ExistingService}", service, service.Id, svc);
        continue;
      }
      Logger?.LogDebug("Adding Service {Service} to config", service);
      Services[service.Id] = service;
    }

    Logger?.LogDebug("Collecting vehicle types");
    foreach (var service in Services.Values) {
      foreach (var vehicle in service.Vehicles) {
        if (!Vehicles.TryGetValue(vehicle, out var count)) {
          Logger?.LogTrace("New Vehicle type {VehicleType}", vehicle);
          count = 0;
        }
        Vehicles[vehicle] = count + 1;
      }
    }

    Logger?.LogInformation("EBuLa Config {Config} fully initialized", this);
    Initialized = true;
    _jConfig = null;
    IsDirty = false;
  }

  public bool ResolveDependencies(Dictionary<string, EbulaConfig> loadedConfigs) {
    Logger?.LogDebug("Resolving dependencies for {config}", this);

    foreach (var dependency in Dependencies) {
      // Dependency loaded
      if (loadedConfigs.FirstOrDefault(c => c.Value.Id == dependency.Key).Value is EbulaConfig dep) {
        // Dependency available
        if (dep.Available || dep.ResolveDependencies(loadedConfigs)) {
          ResolvedDependencies[dependency.Key] = dep;
          continue;
        }

        return false;
      }

      if (!ResolveDependency(dependency.Key, dependency.Value, loadedConfigs))
        return false; // Failed to resolve dependency
    }

    if (_originFileName is not null)
      Save(_originFileName); // To update dependency names

    // All dependencies resolved
    return true;
  }

  private bool ResolveDependency(Guid id, string name, Dictionary<string, EbulaConfig> loadedConfigs) {
    while (true) {
      var dialog = new OpenFileDialog {
        Title = $"Select file for EBuLa config {name}",
        FileName = "vEBuLa",
        DefaultExt = ".ebula",
        Filter = "vEBuLa config files|*.ebula|Json-Files|*.json|All Files|*.*",
        InitialDirectory = App.ConfigFolder
      };

      if (dialog.ShowDialog() != true) return false;
      Logger?.LogInformation("Loading EBuLa Config {ConfigFile}", dialog.FileName.BiCrop(10, 30));


      if (LoadID(dialog.FileName) == id) {
        var newConfig = new EbulaConfig(dialog.FileName);
        loadedConfigs.Add(dialog.FileName, newConfig);
        ResolvedDependencies.Add(id, newConfig);

        return newConfig.ResolveDependencies(loadedConfigs);
      }
      return false;
    }
  }

  private static Guid LoadID(string fileName) {
    var jConfig = JObject.Parse(File.ReadAllText(fileName));

    if (jConfig.Value<string>(nameof(Id)) is string idString)
      return Guid.Parse(idString);
    else
      return Guid.NewGuid();
  }

  public IEnumerable<EbulaSegment> FindSegments(EbulaStation origin) {
    return Segments.Values.Where(s => s.Origin == origin);
  }

  public EbulaStation AddStation(string name) {
    var id = Guid.NewGuid();
    while (Stations.ContainsKey(id)) id = Guid.NewGuid();
    var station = new EbulaStation(this, id, name);
    Stations.Add(id, station);
    IsDirty = true;
    return station;
  }

  public EbulaSegment AddSegment(string name, EbulaStation? origin) {
    var id = Guid.NewGuid();
    while (Segments.ContainsKey(id)) id = Guid.NewGuid();
    var segment = new EbulaSegment(this, id, name);
    if (origin is not null) segment.Origin = origin;
    Segments.Add(id, segment);
    IsDirty = true;
    return segment;
  }

  public EbulaRoute AddRoute(IEnumerable<EbulaSegment> segments, string routeName, string description, int stations, TimeSpan duration, string routeDesc) {
    var id = Guid.NewGuid();
    while (Routes.ContainsKey(id)) id = Guid.NewGuid();
    var route = new EbulaRoute(this, id, segments, routeName, description, stations, duration, routeDesc);
    Routes.Add(id, route);
    IsDirty = true;
    return route;
  }

  public void Save(string fileName) {
    JObject jConfig = new JObject();
    jConfig[nameof(Id)] = Id;
    jConfig[nameof(Name)] = Name;

    var jDependencies = new JObject();
    foreach (var dependency in Dependencies) {
      var id = dependency.Key;
      var name = ResolvedDependencies.TryGetValue(id, out var r) ? r.Name : dependency.Value;
      jDependencies[id.ToString()] = name;
    }
    jConfig[nameof(Dependencies)] = jDependencies;

    var jStations = new JObject();
    foreach (var station in Stations) jStations[station.Key.ToString()] = JObject.FromObject(station.Value);
    jConfig[nameof(Stations)] = jStations;

    var jSegments = new JObject();
    foreach (var segment in Segments) jSegments[segment.Key.ToString()] = JObject.FromObject(segment.Value);
    jConfig[nameof(Segments)] = jSegments;

    var jRoutes = new JObject();
    foreach (var route in Routes) jRoutes[route.Key.ToString()] = JObject.FromObject(route.Value);
    jConfig[nameof(Routes)] = jRoutes;

    var jServices = new JObject();
    foreach (var service in Services) jServices[service.Key.ToString()] = JObject.FromObject(service.Value);
    jConfig[nameof(Services)] = jServices;

    var jsonString = JsonConvert.SerializeObject(jConfig, Formatting.Indented);
    File.WriteAllText(fileName, jsonString);
    IsDirty = false;
  }

  internal EbulaService AddService(EbulaRoute selectedRoute, string serviceName, TimeSpan startTime, string description, List<string> vehicles) {
    var id = Guid.NewGuid();
    while (Services.ContainsKey(id)) id = Guid.NewGuid();
    var service = new EbulaService(this, id, selectedRoute, serviceName, startTime, description, vehicles);
    Services.Add(id, service);
    IsDirty = true;
    return service;
  }
}
