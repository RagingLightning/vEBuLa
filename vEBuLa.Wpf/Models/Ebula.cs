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
  public bool SingleConfig { get; private set; }
  public EbulaConfig? Config => SingleConfig ? LoadedConfigs.First().Value : null;
  public Dictionary<string, EbulaConfig> LoadedConfigs { get; } = [];

  public Dictionary<Guid, EbulaStation> Stations => LoadedConfigs.Values.SelectMany(c => c.Stations).ToDictionary(s => s.Key, s => s.Value);
  public Dictionary<Guid, EbulaSegment> Segments => LoadedConfigs.Values.SelectMany(c => c.Segments).ToDictionary(s => s.Key, s => s.Value);
  public Dictionary<Guid, EbulaRoute> Routes => LoadedConfigs.Values.SelectMany(c => c.Routes).ToDictionary(s => s.Key, s => s.Value);
  public Dictionary<Guid, EbulaService> Services => LoadedConfigs.Values.SelectMany(c => c.Services).ToDictionary(s => s.Key, s => s.Value);
  public Dictionary<string, int> Vehicles => LoadedConfigs.Values.SelectMany(c => c.Vehicles).GroupBy(e => e.Key).ToDictionary(e => e.Key, e => e.Aggregate(0, (acc, v) => acc + v.Value));

  public Ebula(string? configPath, bool singleConfig) {
    Logger?.LogInformation("Creating new EBuLa Model, single config mode: {singleConfig}", singleConfig);
    SingleConfig = singleConfig;

    if (singleConfig) {
      Logger?.LogInformation("Loading single config {ConfigPath}", configPath);
      if (configPath is null)
        LoadedConfigs.Add(string.Empty, new EbulaConfig());
      else
        LoadedConfigs.Add(configPath, new EbulaConfig(configPath));
    }
    else {
      if (!Directory.Exists(configPath)) throw new InvalidOperationException("Config directory doesn't exist");
      Logger?.LogInformation("Loading config files from {ConfigPath}", configPath);

      foreach (var file in Directory.EnumerateFiles(configPath).Where(p => p.EndsWith(".ebula"))) {
        LoadedConfigs.Add(file, new EbulaConfig(file));
      }

      Logger?.LogInformation("Loaded {ConfigCount} configurations", LoadedConfigs.Count);
      if (LoadedConfigs.Count == 0)
        LoadedConfigs.Add(string.Empty, new EbulaConfig());
    }

    var configsToResolve = new List<EbulaConfig>(LoadedConfigs.Values.Where(c => !c.Available));
    foreach (var config in configsToResolve)
      config.ResolveDependencies(LoadedConfigs);

    foreach (var config in LoadedConfigs)
      config.Value.Initialize();
  }
}
