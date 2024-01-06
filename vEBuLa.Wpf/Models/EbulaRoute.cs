using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using vEBuLa.Models;

namespace vEBuLa;

internal class EbulaRoute {
  private ILogger<EbulaRoute>? Logger => App.GetService<ILogger<EbulaRoute>>();

  public EbulaRoute(EbulaConfig existingConfig, Guid id, JObject jRoute) {
    Id = id;
    Name = jRoute.Value<string>(nameof(Name)) ?? "noname";
    Description = jRoute.Value<string>(nameof(Description)) ?? "No description";
    RouteOverview = jRoute.Value<string>(nameof(RouteOverview)) ?? string.Empty;
    StationCount = jRoute.Value<int>(nameof(StationCount));
    Duration = jRoute.Value<TimeSpan>(nameof(Duration));

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

  public Guid Id { get; }
  public string Name { get; set; }
  public string Description { get; set; }
  public string RouteOverview { get; set; }
  public int StationCount { get; set; }
  public TimeSpan Duration { get; set; }
  public List<EbulaSegment> Segments { get; } = new();
}