using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace vEBuLa.Models;

public class EbulaStation {
  public EbulaStation(Guid id, JObject jStation) {
    Id = id;
    Name = jStation.Value<string>(nameof(Name));
  }

  [JsonIgnore]
  public Guid Id { get; } = Guid.Empty;
  public string Name { get; set; } = string.Empty;
}