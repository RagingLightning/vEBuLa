using Newtonsoft.Json;
using System;

namespace vEBuLa.Models;

/// <summary>
/// Manages a single stop for a <seealso cref="EbulaService"/>
/// 
/// Is paired to an <seealso cref="EbulaEntry"/> of an <seealso cref="EbulaSegment"/>
/// </summary>

public class EbulaServiceStop {

  public EbulaServiceStop(int location, string name, bool bold = false, DateTime? arrival = null, DateTime? departure = null) {
    EntryLocation = location;
    EntryName = name;
    Bold = bold;
    Arrival = arrival;
    Departure = departure;
  }

  public EbulaServiceStop(EbulaEntry entry) {
    EntryLocation = entry.Location;
    EntryName = entry.LocationName;
  }

  [JsonConstructor]
  private EbulaServiceStop() { EntryName = string.Empty; }

  public EbulaServiceStop Copy() => new(EntryLocation, EntryName, Bold, Arrival, Departure);

  public bool IsEmpty() => !Bold && Arrival is null && Departure is null;

  public int EntryLocation { get; set; }
  public string EntryName { get; set; }
  public bool Bold { get; set; }
  public DateTime? Arrival { get; set; }
  public DateTime? Departure { get; set; }
}