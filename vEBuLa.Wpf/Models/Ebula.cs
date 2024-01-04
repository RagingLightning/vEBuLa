using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

namespace vEBuLa.Models;
public class Ebula {
  private List<EbulaSegment> Segments { get; } = new();
  public IEnumerable<EbulaEntry> Entries => from segment in Segments from entry in segment.Entries select entry;

  public Ebula() {
    var config = new EbulaConfig(File.ReadAllText("ebula.json"));
    Segments.Add(config.Segments.First().Value);
  }

  internal EbulaEntry AddEntry(EbulaEntry? prev) {
    var segment = prev is null ? Segments[0] : Segments.First(s => s.Entries.Contains(prev));
    var index = prev is null ? 0 : segment.Entries.IndexOf(prev)+1;
    var newEntry = new EbulaEntry();
    segment.Entries.Insert(index, newEntry);
    return newEntry;
  }

  internal void RemoveEntry(EbulaEntry? entry) {
    if (entry is null) return;
    var segment = Segments.First(s => s.Entries.Contains(entry));
    segment.Entries.Remove(entry);
  }
}
