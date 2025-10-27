using System;
using System.Numerics;
using vEBuLa.Extensions;

namespace vEBuLa.Models;

public class EbulaEntry {
  public override string ToString() => $"{SpeedLimit,3} | {Location,6} | {LocationName.Replace('\n', '|').Crop(15),-17}".Replace('\n',' ');
  public static EbulaEntry PreMarker(EbulaSegment segment) => new() { SpeedLimit = -1, LocationName = $"-- PRE {segment.Origin} --" };
  public static readonly EbulaEntry StartMarker = new() { SpeedLimit = -1, LocationName = "-- START --" };
  public static readonly EbulaEntry PostMarker = new() { SpeedLimit = -1, LocationName = "-- POST --" };

  public int SpeedLimit { get; set; } = 0;
  public bool SpeedSigned { get; set; } = true;
  public int Location { get; set; } = 0;
  public Vector2? GpsLocation { get; set; } = null;
  public bool KilometerBreak { get; set; } = false;
  public EbulaSymbol Symbol { get; set; } = EbulaSymbol.NONE;
  public string LocationName { get; set; } = string.Empty;
  public bool LocationNameBold { get; set; } = false;
  public string LocationNotes { get; set; } = string.Empty;
  public bool LocationNotesBold { get; set; } = false;
  public Gradient Gradient { get; set; } = Gradient.BELOW_10;
  public bool TunnelStart { get; set; } = false;
  public bool TunnelEnd { get; set; } = false;
  public bool LabelBox { get; set; } = false;
}
