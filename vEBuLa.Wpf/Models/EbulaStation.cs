using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using vEBuLa.Extensions;

namespace vEBuLa.Models;

public class EbulaStation {
  public override string ToString() => $"{Id.ToString().BiCrop(5,5)} | {Name.Crop(30)}";

  public EbulaStation(EbulaConfig config, Guid id, JObject jStation) {
    Config = config;
    Id = id;
    Name = jStation.Value<string>(nameof(Name)) ?? string.Empty;
  }

  public EbulaStation(EbulaConfig config, Guid id, string name) {
    Config = config;
    Id = id;
    Name = name;
  }

  [JsonIgnore] public EbulaConfig Config { get; }
  [JsonIgnore] public Guid Id { get; } = Guid.Empty;
  public string Name { get; set; } = string.Empty;
}