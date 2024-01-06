using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Policy;

namespace vEBuLa.Models;
internal class Ebula {
  public EbulaConfig? Config { get; }
  public List<EbulaSegment> Segments { get; } = new();
  public TimeSpan ServiceStartTime { get; private set; } = TimeSpan.Zero;

  public Ebula(string? configPath) {
    if (configPath is null) Config = new EbulaConfig();
    else Config = new EbulaConfig(File.ReadAllText(configPath));
  }

  internal void SetActiveSegments(IEnumerable<EbulaSegment> segments, TimeSpan departure) {
    ServiceStartTime = departure;
    Segments.Clear();
    Segments.AddRange(segments);
  }
}
