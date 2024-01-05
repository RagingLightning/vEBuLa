using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace vEBuLa.Models;
public class EbulaSegment {
  private readonly ILogger<EbulaSegment>? logger;
  public override string ToString() => $"{Id,6}: {Origin.Station?.Name,6} > {Destination.Station?.Name,6}";

  public EbulaSegment(EbulaConfig existingConfig, Guid id, JObject jSegment) {
    logger = App.GetService<ILogger<EbulaSegment>>();
    Id = id;
    if (Guid.TryParse(jSegment.Value<string>(nameof(Origin)), out var originKey)) {
      if (existingConfig.Stations.TryGetValue(originKey, out var origin))
        Origin = (originKey, origin);
      else {
        logger?.LogWarning("No Origin Station with Id {StationId} found!", originKey);
        Origin = (originKey, null);
      }
    }

    if (Guid.TryParse(jSegment.Value<string>(nameof(Destination)), out var destinationKey)) {
      if (existingConfig.Stations.TryGetValue(destinationKey, out var destination))
        Destination = (destinationKey, destination);
      else {
        logger?.LogWarning("No Destination Station with Id {StationId} found!", destinationKey);
        Origin = (destinationKey, null);
      }
    }

    Duration = jSegment.Value<TimeSpan>(nameof(Duration));

    if (jSegment.Value<JArray>(nameof(PreEntries)) is JArray jPreEntries) {
      logger?.LogDebug("Parsing PreEntries from {EntryJsonArray}", jPreEntries);
      ParseEntries(PreEntries, jPreEntries);
    }
    else
      logger?.LogWarning("Segment contains no PreEntries");

    if (jSegment.Value<JArray>(nameof(Entries)) is JArray jEntries) {
      logger?.LogDebug("Parsing Entries from {EntryJsonArray}", jEntries);
      ParseEntries(Entries, jEntries);
    }
    else
      logger?.LogWarning("Segment contains no Entries");

    if (jSegment.Value<JArray>(nameof(PostEntries)) is JArray jPostEntries) {
      logger?.LogDebug("Parsing PostEntries from {EntryJsonArray}", jPostEntries);
      ParseEntries(PostEntries, jPostEntries);
    }
    else
      logger?.LogWarning("Segment contains no PostEntries");


  }

  private void ParseEntries(List<EbulaEntry> list, JArray jArray) {
    foreach (var jEntry in jArray) {
      if (jEntry.ToObject<EbulaEntry>() is not EbulaEntry entry) {
        logger?.LogWarning("Entry {EntryJson} failed to parse", jEntry);
        continue;
      }
      logger?.LogTrace("Entry {Entry} parsed successfully", entry);
      list.Add(entry);
    }
  }

  public (List<EbulaEntry> List, int Index)? FindEntry(EbulaEntry entry) {
    if (Entries.Contains(entry))
      return (Entries, Entries.IndexOf(entry));
    if (PreEntries.Contains(entry))
      return (PreEntries, Entries.IndexOf(entry));
    if (PostEntries.Contains(entry))
      return (PostEntries, PostEntries.IndexOf(entry));
    return null;
  }

  public Guid Id { get; } = Guid.Empty;
  public (Guid Key, EbulaStation? Station) Origin { get; set; } = (Guid.Empty, null);
  public (Guid Key, EbulaStation? Station) Destination { get; set; } = (Guid.Empty, null);
  public TimeSpan Duration = new TimeSpan(0, 0, 0);
  public List<EbulaEntry> PreEntries { get; } = new();
  public List<EbulaEntry> Entries { get; } = new();
  public List<EbulaEntry> PostEntries { get; } = new();
}
