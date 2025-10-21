using Newtonsoft.Json;
using System;

namespace vEBuLa.Models;

/// <summary>
/// Manages a single stop for a <seealso cref="EbulaService"/>
/// 
/// Is paired to an <seealso cref="EbulaEntry"/> of an <seealso cref="EbulaSegment"/>
/// </summary>

internal class EbulaServiceStop {

  public EbulaServiceStop(int entryIndex, bool bold = false, DateTime? arrival = null, DateTime? departure = null) {
    EntryIndex = entryIndex;
    Bold = bold;
    Arrival = arrival;
    Departure = departure;
  }

  public EbulaServiceStop Copy() => new(EntryIndex, Bold, Arrival, Departure);

  public bool IsEmpty() => !Bold && Arrival is null && Departure is null;

  public int EntryIndex { get; set; }
  public bool Bold { get; set; }
  public DateTime? Arrival { get; set; }
  public DateTime? Departure { get; set; }
}