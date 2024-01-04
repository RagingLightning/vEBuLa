using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace vEBuLa.Models;
public class EbulaSegment {
  public override string ToString() => $"{Id,6}: {Origin,6} > {Destination,6}";

  public EbulaSegment(Guid id, JObject jSegment) {
    Id = id;
    Origin = Guid.Parse(jSegment.Value<string>(nameof(Origin)));
    Destination = Guid.Parse(jSegment.Value<string>(nameof(Destination)));
    var jEntries = jSegment.Value<JArray>(nameof(Entries));

    foreach (var jEntry in jEntries) {
      if (jEntry is not JObject) continue;
      if (jEntry.ToObject<EbulaEntry>() is not EbulaEntry entry) entry = new EbulaEntry { LocationName = "-- ERROR --" };
      Entries.Add(entry);
    }
  }
  public Guid Id { get; } = Guid.Empty;
  public Guid Origin { get; set; } = Guid.Empty;
  public Guid Destination { get; set; } = Guid.Empty;
  public List<EbulaEntry> Entries { get; } = new();
}
