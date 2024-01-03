using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace vEBuLa.Models;
public class EbulaSegment {
  public override string ToString() => $"{Id,6}: {Origin,6} > {Destination,6}";
  public Guid Id { get; } = Guid.Empty;
  public Guid Origin { get; set; } = Guid.Empty;
  public Guid Destination { get; set; } = Guid.Empty;
  public List<EbulaEntry> Entries { get; } = new();

  public EbulaSegment(Guid id) {
    Id = id;
  }

  public EbulaSegment(JObject jSegment) {
    Id = jSegment.Value<Guid>(nameof(Id));
    Origin = jSegment.Value<Guid>(nameof(Origin));
    Destination = jSegment.Value<Guid>(nameof(Destination));
    var jEntries = jSegment.Value<JArray>(nameof(Entries));

    foreach (var jEntry in jEntries) {
      if (jEntry is not JObject) continue;
      if (jEntry.ToObject<EbulaEntry>() is not EbulaEntry entry) entry = new EbulaEntry { LocationName = "-- ERROR --" };
      Entries.Add(entry);
    }
  }
}
