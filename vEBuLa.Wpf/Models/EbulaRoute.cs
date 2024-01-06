using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using vEBuLa.Models;
using vEBuLa.ViewModels;

namespace vEBuLa;

internal class EbulaRoute {
  private ILogger<EbulaRoute>? Logger => App.GetService<ILogger<EbulaRoute>>();

  public EbulaRoute(EbulaConfig existingConfig, Guid id, JObject jRoute) {
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
        if (!existingConfig.Segments.TryGetValue(segmentId, out var segment)) {
          Logger?.LogWarning("No Segment with Id {SegmentId} found!", segmentId);
          continue;
        }
        Segments.Add(segment);
      }
    }
    else
      Logger?.LogWarning("Route {Route} contains no Segments", this);

  }

  public EbulaRoute(Guid id, IEnumerable<EbulaSegment> segments, string routeName, string description, int stations, TimeSpan duration, string routeDesc) {
    Id = id;
    Description = description;
    Duration = duration;
    Name = routeName;
    Segments = segments.ToList();
    StationCount = stations;
    RouteOverview = routeDesc;
  }

  [JsonIgnore] public Guid Id { get; }
  public string Name { get; set; }
  public string Description { get; set; }
  public string RouteOverview { get; set; }
  public int StationCount { get; set; }
  public TimeSpan Duration { get; set; }
  [JsonProperty(nameof(Segments))] private IEnumerable<Guid> SegmentIds => Segments.Select(e => e.Id);
  [JsonIgnore] public List<EbulaSegment> Segments { get; } = new();
  
}