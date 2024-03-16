using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Policy;

namespace vEBuLa.Models;
internal class Ebula {
  private ILogger<Ebula>? Logger => App.GetService<ILogger<Ebula>>();
  public EbulaConfig Config { get; }
  public List<EbulaSegment> Segments { get; } = new();

  public Ebula(string? configPath) {
    Logger?.LogInformation("Creating new EBuLa Model");
    if (configPath is null) Config = new EbulaConfig();
    else Config = new EbulaConfig(configPath);
  }

  internal void SetActiveSegments(IEnumerable<EbulaSegment> segments) {
    Segments.Clear();
    Segments.AddRange(segments);
  }
}
