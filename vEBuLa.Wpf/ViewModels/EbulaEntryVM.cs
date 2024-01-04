using System;
using vEBuLa.Commands;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;
public class EbulaEntryVM : BaseVM {
  public EbulaEntry? Model { get; private set; }
  public static EbulaEntryVM EditEntry = new();

  private EbulaEntryVM() {
    Model = null;
    TrueEntry = false;
    MainLabel = "--Start--";
  }
  public EbulaEntryVM(EbulaEntry entry, TimeSpan serviceStart, EbulaEntryVM? prev) {
    Model = entry;
    if (prev == EditEntry) prev = null;
    Location = entry.Location;

    MainLabel = entry.LocationName;
    MainBold = entry.LocationNameBold;
    SecondaryLabel = entry.LocationNotes;
    SecondaryBold = entry.LocationNotesBold;
    ArrivalBold = entry.LocationNameBold || entry.LocationNotesBold;
    DepartureBold = entry.LocationNameBold || entry.LocationNotesBold;

    SpeedLimit = entry.SpeedLimit > 0 ? entry.SpeedLimit : prev?.SpeedLimit ?? 0;
    SpeedLimitDisplay = SpeedLimit != (prev?.SpeedLimit ?? 0);

    switch (entry.Symbol) {
      case EbulaSymbol.WEICHENBEREICH:
        YMarker = true; break;
      case EbulaSymbol.ZUGFUNK:
        PhoneMarker = true; break;
      case EbulaSymbol.BERMSWEG_KURZ:
        TriangleMarker = true; break;
      case EbulaSymbol.STUMPFGLEIS:
        TMarker = true; break;
    }

    switch (entry.GradientMark) {
      case Models.Gradient.ABOVE_30: Gradient = 3; break;
      case Models.Gradient.BELOW_30: Gradient = 2; break;
      case Models.Gradient.BELOW_20: Gradient = 1; break;
    }

    if (entry.Arrival is TimeSpan) Arrival = serviceStart.Add(entry.Arrival.Value);

    if (entry.Departure is TimeSpan) Departure = serviceStart.Add(entry.Departure.Value);

    TunnelStart = entry.TunnelStart;
    TunnelEnd = entry.TunnelEnd;
    TunnelMid = TunnelEnd || prev is null ? false : prev.TunnelMid || prev.TunnelStart;
  }


  #region Properties

  #region Edit Mode
  private bool _trueEntry = true;
  public bool TrueEntry {
    get {
      return _trueEntry;
    }
    set {
      _trueEntry = value;
      OnPropertyChanged(nameof(TrueEntry));
    }
  }
  #endregion

  #region Column 1 - Speeds
  public bool SpeedColumn2 => SpeedLimit < 40;
  public bool SpeedColumn4 => SpeedLimit > 39 && SpeedLimit < 60;
  public bool SpeedColumn6 => SpeedLimit > 59 && SpeedLimit < 80;
  public bool SpeedColumn8 => SpeedLimit > 79 && SpeedLimit < 100;
  public bool SpeedColumn10 => SpeedLimit > 99 && SpeedLimit < 120;
  public bool SpeedColumn12 => SpeedLimit > 119 && SpeedLimit < 140;
  public bool SpeedColumn14 => SpeedLimit > 139 && SpeedLimit < 160;
  public bool SpeedColumn16 => SpeedLimit > 159;

  private int _speedLimit = 0;
  public int SpeedLimit {
    get {
      return _speedLimit;
    }
    set {
      _speedLimit = value;
      OnPropertyChanged(nameof(SpeedColumn2));
      OnPropertyChanged(nameof(SpeedColumn4));
      OnPropertyChanged(nameof(SpeedColumn6));
      OnPropertyChanged(nameof(SpeedColumn8));
      OnPropertyChanged(nameof(SpeedColumn10));
      OnPropertyChanged(nameof(SpeedColumn12));
      OnPropertyChanged(nameof(SpeedColumn14));
      OnPropertyChanged(nameof(SpeedColumn16));
      OnPropertyChanged(nameof(SpeedLimitText));
    }
  }

  private bool _speedLimitDisplay;
  public bool SpeedLimitDisplay {
    get {
      return _speedLimitDisplay;
    }
    set {
      _speedLimitDisplay = value;
      OnPropertyChanged(nameof(SpeedLimitText));
    }
  }

  public string SpeedLimitText => SpeedLimitDisplay ? Math.Max(SpeedLimit, 160).ToString() : string.Empty;

  #endregion

  #region Column 2 - Location
  private bool _isCurrent = false;
  public bool IsCurrent {
    get {
      return _isCurrent;
    }
    set {
      _isCurrent = value;
      OnPropertyChanged(nameof(IsCurrent));
    }
  }

  private int _location;
  public int Location {
    get {
      return _location;
    }
    set {
      _location = value;
      OnPropertyChanged(nameof(Location));
      OnPropertyChanged(nameof(LocationInt));
      OnPropertyChanged(nameof(LocationFrac));
    }
  }
  public string LocationInt => (Location / 1000).ToString();
  public string LocationFrac => (Location % 1000 / 100).ToString();

  public bool ZigZag1 => Gradient > 0;
  public bool ZigZag2 => Gradient > 1;
  public bool ZigZag3 => Gradient > 2;

  private int _gradient = 0;
  public int Gradient {
    get {
      return _gradient;
    }
    set {
      _gradient = value;
      OnPropertyChanged(nameof(Gradient));
      OnPropertyChanged(nameof(ZigZag1));
      OnPropertyChanged(nameof(ZigZag2));
      OnPropertyChanged(nameof(ZigZag3));
    }
  }

  private bool _kilometerBreak;
  public bool KilometerBreak {
    get {
      return _kilometerBreak;
    }
    set {
      _kilometerBreak = value;
      OnPropertyChanged(nameof(KilometerBreak));
    }
  }
  #endregion

  #region Column 3 - Main

  private bool _tunnelStart = false;
  public bool TunnelStart {
    get {
      return _tunnelStart;
    }
    set {
      if (value) {
        TunnelMid = false;
        TunnelEnd = false;
      }
      _tunnelStart = value;
      OnPropertyChanged(nameof(TunnelStart));
    }
  }

  private bool _tunnelMid = false;
  public bool TunnelMid {
    get {
      return _tunnelMid;
    }
    set {
      if (value) {
        TunnelStart = false;
        TunnelEnd = false;
      }
      _tunnelMid = value;
      OnPropertyChanged(nameof(TunnelMid));
    }
  }

  private bool _tunnelEnd = false;
  public bool TunnelEnd {
    get {
      return _tunnelEnd;
    }
    set {
      if (value) {
        TunnelStart = false;
        TunnelMid = false;
      }
      _tunnelEnd = value;
      OnPropertyChanged(nameof(TunnelEnd));
    }
  }

  private bool _yMarker = false;
  public bool YMarker {
    get {
      return _yMarker;
    }
    set {
      _yMarker = value;
      OnPropertyChanged(nameof(YMarker));
    }
  }

  private bool _phoneMarker = false;
  public bool PhoneMarker {
    get {
      return _phoneMarker;
    }
    set {
      _phoneMarker = value;
      OnPropertyChanged(nameof(PhoneMarker));
    }
  }

  private bool _triangleMarker = false;
  public bool TriangleMarker {
    get {
      return _triangleMarker;
    }
    set {
      _triangleMarker = value;
      OnPropertyChanged(nameof(TriangleMarker));
    }
  }

  private bool _tMarker = false;
  public bool TMarker {
    get {
      return _tMarker;
    }
    set {
      _tMarker = value;
      OnPropertyChanged(nameof(TMarker));
    }
  }

  private string _mainLabel = string.Empty;
  public string MainLabel {
    get {
      return _mainLabel;
    }
    set {
      _mainLabel = value;
      OnPropertyChanged(nameof(MainLabel));
    }
  }

  private bool _mainBold = false;
  public bool MainBold {
    get {
      return _mainBold;
    }
    set {
      _mainBold = value;
      OnPropertyChanged(nameof(MainBold));
    }
  }

  private string _secondaryLabel = string.Empty;
  public string SecondaryLabel {
    get {
      return _secondaryLabel;
    }
    set {
      _secondaryLabel = value;
      OnPropertyChanged(nameof(SecondaryLabel));
    }
  }

  private bool _secondaryBold = false;
  public bool SecondaryBold {
    get {
      return _secondaryBold;
    }
    set {
      _secondaryBold = value;
      OnPropertyChanged(nameof(SecondaryBold));
    }
  }

  #endregion

  #region Column 4 - Arrival

  private bool _arrivalBold;
  public bool ArrivalBold {
    get {
      return _arrivalBold;
    }
    set {
      _arrivalBold = value;
      OnPropertyChanged(nameof(ArrivalBold));
    }
  }

  private TimeSpan? _arrival = null;
  public TimeSpan? Arrival {
    get {
      return _arrival;
    }
    set {
      _arrival = value;
      OnPropertyChanged(nameof(Arrival));
      OnPropertyChanged(nameof(HasArrival));
      OnPropertyChanged(nameof(ArrivalHr));
      OnPropertyChanged(nameof(ArrivalMn));
      OnPropertyChanged(nameof(ArrivalFr));
    }
  }
  public bool HasArrival => Arrival is not null;
  public string ArrivalHr => Arrival?.Hours.ToString("00") ?? string.Empty;
  public string ArrivalMn => Arrival?.Minutes.ToString("00") ?? string.Empty;
  public string ArrivalFr => (Arrival?.Seconds / 6)?.ToString("0") ?? string.Empty;

  #endregion

  #region Column 5 - Departure

  private bool _departureBold;
  public bool DepartureBold {
    get {
      return _departureBold;
    }
    set {
      _departureBold = value;
      OnPropertyChanged(nameof(DepartureBold));
    }
  }

  private TimeSpan? _departure = null;
  public TimeSpan? Departure {
    get {
      return _departure;
    }
    set {
      _departure = value;
      OnPropertyChanged(nameof(Departure));
      OnPropertyChanged(nameof(HasDeparture));
      OnPropertyChanged(nameof(DepartureHr));
      OnPropertyChanged(nameof(DepartureMn));
      OnPropertyChanged(nameof(DepartureFr));
    }
  }
  public bool HasDeparture => Departure is not null;
  public string DepartureHr => Departure?.Hours.ToString("00") ?? string.Empty;
  public string DepartureMn => Departure?.Minutes.ToString("00") ?? string.Empty;
  public string DepartureFr => (Departure?.Seconds / 6)?.ToString("0") ?? string.Empty;

  #endregion

  #endregion
}
