using System;

namespace vEBuLa.Models;
internal class EbulaEntry {
  public static EbulaEntry PreMarker(EbulaSegment segment) => new EbulaEntry{ SpeedLimit = -1, LocationName = $"-- PRE {segment.Origin} --" };
  public static readonly EbulaEntry StartMarker = new EbulaEntry { SpeedLimit = -1, LocationName = "-- START --" };
  public static readonly EbulaEntry PostMarker = new EbulaEntry { SpeedLimit = -1, LocationName = "-- POST --" };

  public int SpeedLimit { get; set; } = 0;
  public bool SpeedSigned { get; set; } = true;
  public int Location { get; set; } = 0;
  public bool KilometerBreak { get; set; } = false;
  public EbulaSymbol Symbol { get; set; } = EbulaSymbol.NONE;
  public string LocationName { get; set; } = string.Empty;
  public bool LocationNameBold { get; set; } = false;
  public string LocationNotes { get; set; } = string.Empty;
  public bool LocationNotesBold { get; set; } = false;
  public Gradient Gradient { get; set; } = Gradient.BELOW_10;
  public TimeSpan? Arrival { get; set; } = null;
  public TimeSpan? Departure { get; set; } = null;
  public bool TunnelStart { get; set; } = false;
  public bool TunnelEnd { get; set; } = false;
}
