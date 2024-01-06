using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace vEBuLa.Models;
internal class EbulaSegment {
  private readonly ILogger<EbulaSegment>? Logger;
  public override string ToString() => $"{Id,6}: {Origin.Station?.Name,6} > {Destination.Station?.Name,6}";

  public EbulaSegment(EbulaConfig existingConfig, Guid id, JObject jSegment) {
    Logger = App.GetService<ILogger<EbulaSegment>>();
    Id = id;
    if (Guid.TryParse(jSegment.Value<string>(nameof(Origin)), out var originKey)) {
      if (existingConfig.Stations.TryGetValue(originKey, out var origin))
        Origin = (originKey, origin);
      else {
        Logger?.LogWarning("No Origin Station with Id {StationId} found!", originKey);
        Origin = (originKey, null);
      }
    }

    if (Guid.TryParse(jSegment.Value<string>(nameof(Destination)), out var destinationKey)) {
      if (existingConfig.Stations.TryGetValue(destinationKey, out var destination))
        Destination = (destinationKey, destination);
      else {
        Logger?.LogWarning("No Destination Station with Id {StationId} found!", destinationKey);
        Origin = (destinationKey, null);
      }
    }

    if (TimeSpan.TryParse(jSegment.Value<string>(nameof(Duration)), out var duration)) {
      Duration = duration;
    }
    else
      Logger?.LogWarning("Failed to parse segment duration {Duration}", jSegment.Value<string>(nameof(Duration)));
    Name = jSegment.Value<string>(nameof(Name)) ?? string.Empty;

    if (jSegment.Value<JArray>(nameof(PreEntries)) is JArray jPreEntries) {
      Logger?.LogDebug("Parsing PreEntries for {Segment}", this);
      ParseEntries(PreEntries, jPreEntries);
    }
    else
      Logger?.LogWarning("Segment {Segment} contains no PreEntries", this);

    if (jSegment.Value<JArray>(nameof(Entries)) is JArray jEntries) {
      Logger?.LogDebug("Parsing Entries for {Segment}", this);
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
    Id = id;
    Name = name;
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

  [JsonIgnore] public Guid Id { get; } = Guid.Empty;
  public string Name { get; set; } = string.Empty;
  [JsonProperty(nameof(Origin))] private Guid OriginId => Origin.Key;
  [JsonIgnore] public (Guid Key, EbulaStation? Station) Origin { get; set; } = (Guid.Empty, null);
  [JsonProperty(nameof(Destination))] private Guid DestinationId => Destination.Key;
  [JsonIgnore] public (Guid Key, EbulaStation? Station) Destination { get; set; } = (Guid.Empty, null);
  public TimeSpan Duration { get; set; } = new TimeSpan(0, 0, 0);
  public List<EbulaEntry> PreEntries { get; } = new();
  public List<EbulaEntry> Entries { get; } = new();
  public List<EbulaEntry> PostEntries { get; } = new();
}
