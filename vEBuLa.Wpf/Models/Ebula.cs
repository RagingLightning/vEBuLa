using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using vEBuLa.Views;

namespace vEBuLa.Models;

public class Ebula {
  private ILogger<Ebula>? Logger => App.GetService<ILogger<Ebula>>();
  public EbulaConfig? Config => LoadedConfigs.Count == 1 ? LoadedConfigs[0] : null;
  public List<EbulaConfig> LoadedConfigs { get; } = new();

  public Ebula(string? configPath, bool singleConfig) {
    Logger?.LogInformation("Creating new EBuLa Model");
    if (singleConfig) {
      Logger?.LogInformation("Loading single config {ConfigPath}", configPath);
      if (configPath is null) LoadedConfigs.Add(new EbulaConfig());
      else LoadedConfigs.Add(new EbulaConfig(configPath));
    } else {
      if (!Directory.Exists(configPath)) throw new InvalidOperationException("Config directory doesn't exist");
      Logger?.LogInformation("Loading config files from {ConfigPath}", configPath);
      LoadedConfigs.AddRange(Directory.EnumerateFiles(configPath).Where(p => p.EndsWith(".ebula")).Select(s => new EbulaConfig(s)));
      Logger?.LogInformation("Loaded {ConfigCount} configurations", LoadedConfigs.Count);
      if (LoadedConfigs.Count == 0) LoadedConfigs.Add(new EbulaConfig());
    }
  }
}
