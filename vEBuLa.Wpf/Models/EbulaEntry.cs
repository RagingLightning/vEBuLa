﻿using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;

namespace vEBuLa.Models;
public class EbulaEntry {

  public int SpeedLimit { get; set; } = 0;
  public bool SpeedSigned { get; set; } = true;
  public int Location { get; set; } = 0;
  public bool KilometerBreak { get; set; } = false;

  [DefaultValue(EbulaSymbol.NONE)]
  public EbulaSymbol Symbol { get; set; } = EbulaSymbol.NONE;
  public string LocationName { get; set; } = string.Empty;
  public bool LocationNameBold { get; set; } = false;
  public string LocationNotes { get; set; } = string.Empty;
  public bool LocationNotesBold { get; set; } = false;

  [DefaultValue(Gradient.BELOW_10)]
  public Gradient GradientMark { get; set; } = Gradient.BELOW_10;
  public TimeSpan? Arrival { get; set; } = null;
  public TimeSpan? Departure { get; set; } = null;
  public bool TunnelStart { get; set; } = false;
  public bool TunnelEnd { get; set; } = false;
}
