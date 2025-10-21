using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using vEBuLa.Commands;
using vEBuLa.Extensions;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;
internal class EbulaEntryVM : BaseVM {
  private ILogger<EbulaEntryVM>? Logger => App.GetService<ILogger<EbulaEntryVM>>();
  public EbulaEntry Model { get; }
  public EbulaScreenVM Screen { get; }
  public EbulaServiceStop Stop { get; }
  private DateTime ServiceStartTime { get; }

  public override string ToString() => $"{Model.Location,6} | {Model.LocationName.Crop(16),-16} | {(Arrival is null ? "       " : $"{ArrivalHr}:{ArrivalMn}.{ArrivalFr}")} | {(Departure is null ? "       " : $"{DepartureHr}:{DepartureMn}.{DepartureFr}")}";

  public EbulaEntryVM(EbulaEntry entry, EbulaEntryVM? prev, EbulaServiceStop stop, DateTime serviceStartTime, EbulaScreenVM screen) {
    Model = entry;
    Screen = screen;
    Stop = stop;
    ServiceStartTime = serviceStartTime;

    DisplayUnits = entry.LocationName.Where(c => c == '\n').Count() + 1;

    EditSpeedCommand = EditEbulaEntrySpeedC.INSTANCE;
    EditLocationCommand = EditEbulaEntryLocationC.INSTANCE;
    EditTunnelCommand = EditEbulaEntryTunnelC.INSTANCE;
    EditSymbolCommand = EditEbulaEntrySymbolC.INSTANCE;
    EditStationCommand = EditEbulaEntryStationC.INSTANCE;
    EditArrivalCommand = EditEbulaEntryTimeC.ARRIVAL;
    EditDepartureCommand = EditEbulaEntryTimeC.DEPARTURE;

    PrevSpeedLimit = prev?.SpeedLimit ?? 0;
    PrevTunnel = prev is null ? false : prev.TunnelMid.Count > 0 || prev.TunnelStart;

    MainLabels.AddRange(entry.LocationName.Split('\n').Select(s => s.Replace("\r", null)));
    SetGradient();
    SetTunnel();
  }

  private void SetGradient() {
    if (Gradient < Gradient.BELOW_20)
      ZigZag1.Clear();
    else
      while (ZigZag1.Count < DisplayUnits)
        ZigZag1.Add(new());
    OnPropertyChanged(nameof(ZigZag1));
    if (Gradient < Gradient.BELOW_30)
      ZigZag2.Clear();
    else
      while (ZigZag2.Count < DisplayUnits)
        ZigZag2.Add(new());
    OnPropertyChanged(nameof(ZigZag2));
    if (Gradient < Gradient.ABOVE_30)
      ZigZag3.Clear();
    else
      while (ZigZag3.Count < DisplayUnits)
        ZigZag3.Add(new());
    OnPropertyChanged(nameof(ZigZag3));
  }

  private void SetTunnel() {
    if (TunnelStart || TunnelEnd || !PrevTunnel)
      TunnelMid.Clear();
    else
      while (TunnelMid.Count < DisplayUnits)
        TunnelMid.Add(new());
  }

  #region Properties

  #region Edit Mode

  #region Commands
  public BaseC EditSpeedCommand { get; }
  public BaseC EditLocationCommand { get; }
  public BaseC EditTunnelCommand { get; }
  public BaseC EditSymbolCommand { get; }
  public BaseC EditStationCommand { get; }
  public BaseC EditArrivalCommand { get; }
  public BaseC EditDepartureCommand { get; }
  #endregion
  #endregion

  public int DisplayUnits { get; }

  public int Height => 31 * DisplayUnits;

  #region Column 1 - Speeds
  public bool SpeedColumn1 => SpeedLimit > 0 && SpeedLimit < 30;
  public bool SpeedColumn2 => SpeedLimit > 19 && SpeedLimit < 40;
  public bool SpeedColumn3 => SpeedLimit > 29 && SpeedLimit < 50;
  public bool SpeedColumn4 => SpeedLimit > 39 && SpeedLimit < 60;
  public bool SpeedColumn5 => SpeedLimit > 49 && SpeedLimit < 70;
  public bool SpeedColumn6 => SpeedLimit > 59 && SpeedLimit < 80;
  public bool SpeedColumn7 => SpeedLimit > 69 && SpeedLimit < 90;
  public bool SpeedColumn8 => SpeedLimit > 79 && SpeedLimit < 100;
  public bool SpeedColumn9 => SpeedLimit > 89 && SpeedLimit < 110;
  public bool SpeedColumn10 => SpeedLimit > 99 && SpeedLimit < 120;
  public bool SpeedColumn11 => SpeedLimit > 109 && SpeedLimit < 130;
  public bool SpeedColumn12 => SpeedLimit > 119 && SpeedLimit < 140;
  public bool SpeedColumn13 => SpeedLimit > 129 && SpeedLimit < 150;
  public bool SpeedColumn14 => SpeedLimit > 139 && SpeedLimit < 160;
  public bool SpeedColumn15 => SpeedLimit > 149;
  public bool SpeedColumn16 => SpeedLimit > 159;

  public int PrevSpeedLimit { get; } = 0;
  public int SpeedLimit {
    get {
      return Model.SpeedLimit == 0 ? PrevSpeedLimit : Model.SpeedLimit;
    }
    set {
      Model.SpeedLimit = value;
      OnPropertyChanged(nameof(SpeedColumn1));
      OnPropertyChanged(nameof(SpeedColumn2));
      OnPropertyChanged(nameof(SpeedColumn3));
      OnPropertyChanged(nameof(SpeedColumn4));
      OnPropertyChanged(nameof(SpeedColumn5));
      OnPropertyChanged(nameof(SpeedColumn6));
      OnPropertyChanged(nameof(SpeedColumn7));
      OnPropertyChanged(nameof(SpeedColumn8));
      OnPropertyChanged(nameof(SpeedColumn9));
      OnPropertyChanged(nameof(SpeedColumn10));
      OnPropertyChanged(nameof(SpeedColumn11));
      OnPropertyChanged(nameof(SpeedColumn12));
      OnPropertyChanged(nameof(SpeedColumn13));
      OnPropertyChanged(nameof(SpeedColumn14));
      OnPropertyChanged(nameof(SpeedColumn15));
      OnPropertyChanged(nameof(SpeedColumn16));
      OnPropertyChanged(nameof(SpeedLimitText));
    }
  }

  private bool _forceSpeedDisplay = false;
  public bool ForceSpeedDisplay {
    get {
      return _forceSpeedDisplay;
    }
    set {
      _forceSpeedDisplay = value;
      OnPropertyChanged(nameof(SpeedLimitDisplay));
    }
  }

  public bool SpeedLimitDisplay => Model.SpeedLimit > 0 || ForceSpeedDisplay;

  private bool _noSpeedLine;
  public bool NoSpeedLine {
    get {
      return _noSpeedLine;
    }
    set {
      _noSpeedLine = value;
      OnPropertyChanged(nameof(SpeedLineDisplay));
    }
  }
  public bool SpeedLineDisplay => SpeedLimitDisplay && !NoSpeedLine;
  public string SpeedLimitText => SpeedLimitDisplay ? Math.Min(SpeedLimit, 160).ToString() : string.Empty;

  public bool SpeedSigned {
    get {
      return Model.SpeedSigned;
    }
    set {
      Model.SpeedSigned = value;
      OnPropertyChanged(nameof(SpeedSigned));
      OnPropertyChanged(nameof(SpeedColor));
      OnPropertyChanged(nameof(SpeedBackground));
    }
  }

  public Brush SpeedColor => SpeedSigned ? Brushes.Black : Brushes.White;
  public Brush SpeedBackground => ForceSpeedDisplay ? Brushes.Yellow : SpeedSigned ? Brushes.White : Brushes.Black;

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

  public int? Location {
    get {
      return Model.Location;
    }
    set {
      Model.Location = value;
      OnPropertyChanged(nameof(Location));
      OnPropertyChanged(nameof(LocationInt));
      OnPropertyChanged(nameof(LocationFrac));
    }
  }
  public string LocationInt => Location is not int l ? string.Empty : (l / 1000).ToString();
  public string LocationFrac => Location is not int l ? string.Empty : (l % 1000 / 100).ToString();

  public ObservableCollection<object> ZigZag1 { get; } = new();
  public ObservableCollection<object> ZigZag2 { get; } = new();
  public ObservableCollection<object> ZigZag3 { get; } = new();

  public Gradient Gradient {
    get {
      return Model.Gradient;
    }
    set {
      Model.Gradient = value;
      OnPropertyChanged(nameof(Gradient));
      SetGradient();
      OnPropertyChanged(nameof(ZigZag1));
      OnPropertyChanged(nameof(ZigZag2));
      OnPropertyChanged(nameof(ZigZag3));
    }
  }

  public bool KilometerBreak {
    get {
      return Model.KilometerBreak;
    }
    set {
      Model.KilometerBreak = value;
      OnPropertyChanged(nameof(KilometerBreak));
    }
  }
  #endregion

  #region Column 3 - Main

  public bool TunnelStart {
    get {
      return Model.TunnelStart;
    }
    set {
      Model.TunnelStart = value;
      OnPropertyChanged(nameof(TunnelStart));
      SetTunnel();
      OnPropertyChanged(nameof(TunnelMid));
    }
  }

  private bool PrevTunnel = false;
  public ObservableCollection<object> TunnelMid { get; } = new();

  public bool TunnelEnd {
    get {
      return Model.TunnelEnd;
    }
    set {
      Model.TunnelEnd = value;
      OnPropertyChanged(nameof(TunnelEnd));
      SetTunnel();
      OnPropertyChanged(nameof(TunnelMid));
    }
  }

  public EbulaSymbol Symbol {
    get => Model.Symbol;
    set {
      Model.Symbol = value;
      OnPropertyChanged(nameof(Symbol));
      OnPropertyChanged(nameof(YMarker));
      OnPropertyChanged(nameof(PhoneMarker));
      OnPropertyChanged(nameof(TriangleMarker));
      OnPropertyChanged(nameof(TMarker));
    }
  }

  public bool YMarker => Symbol == EbulaSymbol.WEICHENBEREICH;

  public bool PhoneMarker => Symbol == EbulaSymbol.ZUGFUNK;

  public bool TriangleMarker => Symbol == EbulaSymbol.BERMSWEG_KURZ;

  public bool TMarker => Symbol == EbulaSymbol.STUMPFGLEIS;

  public bool LabelBox {
    get => Model.LabelBox;
    set {
      Model.LabelBox = value;
      OnPropertyChanged(nameof(LabelBox));
      OnPropertyChanged(nameof(LabelBoxColor));
    }
  }

  public Brush LabelBoxColor => Model.LabelBox ? Brushes.Black : Brushes.White;

  public string MainLabel {
    get {
      return Model.LocationName;
    }
    set {
      Model.LocationName = value;
      MainLabels.Clear();
      MainLabels.AddRange(value.Split('\n').Select(s => s.Replace("\r", null)));
      OnPropertyChanged(nameof(MainLabel));
    }
  }

  public ObservableCollection<string> MainLabels { get; } = new();

  public bool MainBold {
    get {
      return Model.LocationNameBold || Stop.Bold;
    }
    set {
      if (Screen.Ebula.RouteEditMode)
        Model.LocationNameBold = value;
      else
        Stop.Bold = value;
      OnPropertyChanged(nameof(MainBold));
      OnPropertyChanged(nameof(SecondaryBold));
      OnPropertyChanged(nameof(ArrivalBold));
      OnPropertyChanged(nameof(DepartureBold));
    }
  }

  public string SecondaryLabel {
    get {
      return Model.LocationNotes;
    }
    set {
      Model.LocationNotes = value;
      OnPropertyChanged(nameof(SecondaryLabel));
    }
  }

  public bool SecondaryBold {
    get {
      return Model.LocationNotesBold || Stop.Bold;
    }
    set {
      if (Screen.Ebula.RouteEditMode)
        Model.LocationNotesBold = value;
      else
        Stop.Bold = value;
      OnPropertyChanged(nameof(MainBold));
      OnPropertyChanged(nameof(SecondaryBold));
      OnPropertyChanged(nameof(ArrivalBold));
      OnPropertyChanged(nameof(DepartureBold));
    }
  }

  #endregion

  #region Column 4 - Arrival

  public bool ArrivalBold => MainBold || SecondaryBold;

  public DateTime? Arrival {
    get {
      return Stop.Arrival;
    }
    set {
      Stop.Arrival = value;
      OnPropertyChanged(nameof(Arrival));
      OnPropertyChanged(nameof(HasArrival));
      OnPropertyChanged(nameof(ArrivalHr));
      OnPropertyChanged(nameof(ArrivalMn));
      OnPropertyChanged(nameof(ArrivalFr));
    }
  }
  public bool HasArrival => Arrival is not null;
  public string ArrivalHr => Arrival?.Hour.ToString("00") ?? string.Empty;
  public string ArrivalMn => Arrival?.Minute.ToString("00") ?? string.Empty;
  public string ArrivalFr => (Arrival?.Second / 6)?.ToString("0") ?? string.Empty;

  #endregion

  #region Column 5 - Departure
  public bool DepartureBold => MainBold || SecondaryBold;

  public DateTime? Departure {
    get {
      return Stop.Departure;
    }
    set {
      Stop.Departure = value;
      OnPropertyChanged(nameof(Departure));
      OnPropertyChanged(nameof(HasDeparture));
      OnPropertyChanged(nameof(DepartureHr));
      OnPropertyChanged(nameof(DepartureMn));
      OnPropertyChanged(nameof(DepartureFr));
    }
  }
  public bool HasDeparture => Departure is not null;

  public string DepartureHr => Departure?.Hour.ToString("00") ?? string.Empty;
  public string DepartureMn => Departure?.Minute.ToString("00") ?? string.Empty;
  public string DepartureFr => (Departure?.Second / 6)?.ToString("0") ?? string.Empty;


  #endregion

  #endregion
}
