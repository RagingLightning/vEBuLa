using Newtonsoft.Json.Linq;
using System;

namespace vEBuLa.Models;

public class EbulaStation {
  public Guid Id { get; } = Guid.Empty;
  public string Name { get; set; } = string.Empty;
}