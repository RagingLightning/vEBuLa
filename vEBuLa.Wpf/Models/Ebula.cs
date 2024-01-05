using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

namespace vEBuLa.Models;
public class Ebula {
  public EbulaConfig Config { get; }
  public List<EbulaSegment> Segments { get; } = new();

  public TimeSpan ServiceStartTime { get; } = new(0, 0, 0);

  public Ebula() {
    Config = new EbulaConfig(File.ReadAllText("ebula.json"));
    Segments.Add(Config.Segments.First().Value);
  }
}
