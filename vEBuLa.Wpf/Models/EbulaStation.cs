using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using vEBuLa.Extensions;

namespace vEBuLa.Models;

public class EbulaStation {
  public override string ToString() => $"{Id.ToString().BiCrop(5,5)} | {Name.Crop(30)}";
  public EbulaStation(Guid id, JObject jStation) {
    Id = id;
    Name = jStation.Value<string>(nameof(Name));
  }

  public EbulaStation(Guid id, string name) {
    Id = id;
    Name = name;
  }

  [JsonIgnore] public Guid Id { get; } = Guid.Empty;
  public string Name { get; set; } = string.Empty;
}