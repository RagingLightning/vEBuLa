using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using vEBuLa.Extensions;
using vEBuLa.Models;
using vEBuLa.ViewModels;

namespace vEBuLa;

public class EbulaRoute {
  public override string ToString() => $"{Id.ToString().BiCrop(5, 5)} | {Name.Crop(30)}";
  private ILogger<EbulaRoute>? Logger => App.GetService<ILogger<EbulaRoute>>();

  public EbulaRoute(EbulaConfig config, Guid id, JObject jRoute) {
    Config = config;
    Id = id;
    Name = jRoute.Value<string>(nameof(Name)) ?? "noname";
    Description = jRoute.Value<string>(nameof(Description)) ?? "No description";
    RouteOverview = jRoute.Value<string>(nameof(RouteOverview)) ?? string.Empty;
    StationCount = jRoute.Value<int>(nameof(StationCount));


    if (TimeSpan.TryParse(jRoute.Value<string>(nameof(Duration)), out var duration)) {
      Duration = duration;
    }
    else
      Logger?.LogWarning("Failed to parse segment duration {Duration}", jRoute.Value<string>(nameof(Duration)));

    if (jRoute.Value<JArray>(nameof(Segments)) is JArray jSegments) {
      foreach (var jSegment in jSegments) {
        if (!Guid.TryParse(jSegment.Value<string>(), out var segmentId)) {
          Logger?.LogWarning("Segment ID {SegmentIdJson} failed to parse", jSegment);

        }
        if (!config.Segments.TryGetValue(segmentId, out var segment)) {
          Logger?.LogWarning("No Segment with Id {SegmentId} found!", segmentId);
          continue;
        }
        Segments.Add(segment);
      }
    }
    else
      Logger?.LogWarning("Route {Route} contains no Segments", this);

  }

  public EbulaRoute(EbulaConfig config, Guid id, IEnumerable<EbulaSegment> segments, string routeName, string description, int stations, TimeSpan duration, string routeDesc) {
    Config = config;
    Id = id;
    Description = description;
    Duration = duration;
    Name = routeName;
    Segments = segments.ToList();
    StationCount = stations;
    RouteOverview = routeDesc;
  }

  [JsonIgnore] public EbulaConfig Config { get; }
  [JsonIgnore] public Guid Id { get; }
  public string Name { get; set; }
  public string Description { get; set; }
  public string RouteOverview { get; set; }
  public int StationCount { get; set; }
  public TimeSpan Duration { get; set; }
  [JsonIgnore] public List<EbulaSegment> Segments { get; } = new();
  [JsonProperty(nameof(Segments))] public IEnumerable<string> SegmentIds => Segments.Select(s => {
    if (s.Config == Config) return s.Id.ToString();
    else return $"{s.Config.Id}.{s.Id}";
  });

}