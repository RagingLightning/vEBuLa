using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using vEBuLa.Commands;
using vEBuLa.Extensions;

namespace vEBuLa.ViewModels;
internal class EbulaScreenVM : BaseVM {
  private ILogger<EbulaEntryVM>? Logger => App.GetService<ILogger<EbulaEntryVM>>();
  public override string ToString() => $"EbulaScreen";
  public EbulaVM Ebula { get; }

  public EbulaScreenVM(EbulaVM ebula) {
    Ebula = ebula;
    Ebula.NavigateCommand = new NavigateEbulaScreenC(this);

    AddEntryCommand = new AddEbulaEntryC(ebula);
    RemoveEntryCommand = new RemoveEbulaEntryC(ebula);

    ebula.PropertyChanged += Ebula_PropertyChanged;

    UpdateEntries();
  }

  public override void Destroy() {
    Ebula.PropertyChanged -= Ebula_PropertyChanged;
  }

  public void Ebula_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
    if (sender != Ebula) return;
    if (e.PropertyName == nameof(Ebula.EditMode)) UpdateEntries();
  }

  public void UpdateEntries() {
    Logger?.LogDebug("Updating EbulaScreen entries, with EditMode={EditMode}", Ebula.EditMode);
    Entries.Clear();
    EbulaEntryVM? ebulaEntry = null;
    TimeSpan departureOffset = TimeSpan.Zero;
    if (Ebula.EditMode) {
      var preMarker = new EbulaMarkerEntryVM(this, Ebula.Model.Segments[0], EbulaMarkerType.PRE);
      Entries.Add(preMarker);
      Logger?.LogTrace("Added EbulaMarker {Marker} for Segment {Segment} Pre stage; Count={EntryCount}", preMarker, Ebula.Model.Segments[0], Entries.Count);
    }
    Entries.AddRange(Ebula.Model.Segments[0].PreEntries.Select(e => ebulaEntry = new EbulaEntryVM(e, Ebula.Model.ServiceStartTime + departureOffset, ebulaEntry, this)));
    Logger?.LogTrace("Added {AddCount} EbulaEntries from Segment {Segment} Pre stage; Count={EntryCount}", Ebula.Model.Segments[0].Entries.Count, Ebula.Model.Segments[0], Entries.Count);
    foreach (var segment in Ebula.Model.Segments) {
      if (Ebula.EditMode) {
        var mainMarker = new EbulaMarkerEntryVM(this, segment, EbulaMarkerType.MAIN);
        Entries.Add(mainMarker);
        Logger?.LogTrace("Added EbulaMarker {Marker} for Segment {Segment} Main stage; Count={EntryCount}", mainMarker, segment, Entries.Count);
      }
      Entries.AddRange(segment.Entries.Select(e => ebulaEntry = new EbulaEntryVM(e, Ebula.Model.ServiceStartTime + departureOffset, ebulaEntry, this)));
      departureOffset += segment.Duration;
      Logger?.LogTrace("Added {AddCount} EbulaEntries from Segment {Segment} Main stage; Count={EntryCount}", segment.Entries.Count, segment, Entries.Count);
    }
    if (Ebula.EditMode) {
      var postMarker = new EbulaMarkerEntryVM(this, Ebula.Model.Segments[^1], EbulaMarkerType.POST);
      Entries.Add(postMarker);
      Logger?.LogTrace("Added EbulaMarker {Marker} for Segment {Segment} Post stage; Count={EntryCount}", postMarker, Ebula.Model.Segments[^1], Entries.Count);
    }
    Entries.AddRange(Ebula.Model.Segments[^1].PostEntries.Select(e => ebulaEntry = new EbulaEntryVM(e, Ebula.Model.ServiceStartTime + departureOffset, ebulaEntry, this)));
    Logger?.LogTrace("Added {AddCount} EbulaEntries from Segment {Segment} Post stage; Count={EntryCount}", Ebula.Model.Segments[^1].Entries.Count, Ebula.Model.Segments[^1], Entries.Count);
    if (!Ebula.EditMode)
      CurrentEntry = 0;
    UpdateList();
  }

  public void UpdateList() {
    Logger?.LogDebug("Updating EbulaScreen display list, starting at index {StartIndex}", StartEntry);
    ActiveEntries.Clear();
    ActiveEntries.AddRange(Entries.Skip(StartEntry).Take(15).Reverse());
    CreatePreviewInfo();
  }

  private void CreatePreviewInfo() {
    if (Entries.Count <= StartEntry + 16) {
      SpeedInfo = null;
      StopInfo = null;
    }
    bool speedFound = false, stopFound = false;
    foreach (var entry in Entries.Skip(StartEntry+15)) {
      if (entry is not EbulaEntryVM vm) continue;
      if (!speedFound && vm.SpeedLimit != vm.PrevSpeedLimit) {
        SpeedInfo = $"ab km {vm.LocationInt},{vm.LocationFrac}: {vm.SpeedLimit} km/h";
        speedFound = true;
      }
      if (!stopFound && vm.Arrival is not null) {
        StopInfo = $"Nächster Halt: {vm.MainLabel}";
        stopFound = true;
      }
      if (speedFound && stopFound) break;
    }

    if (!speedFound) SpeedInfo = null;
    if (!stopFound) StopInfo = null;
  }

  #region Properties

  #region Edit Mode
  public BaseC AddEntryCommand { get; }
  public BaseC RemoveEntryCommand { get; }
  #endregion

  #region Popup Windows

  private bool _controlModeOpen;
  public bool ControlModeOpen {
    get {
      return _controlModeOpen;
    }
    set {
      _controlModeOpen = value;
      OnPropertyChanged(nameof(ControlModeOpen));
    }
  }

  #endregion

  #region Header

  private string? _speedInfo;
  public string? SpeedInfo {
    get {
      return _speedInfo;
    }
    set {
      _speedInfo = value;
      OnPropertyChanged(nameof(SpeedInfo));
    }
  }

  private string? _bottomStatus;
  public string? BottomStatus {
    get {
      return _bottomStatus;
    }
    set {
      _bottomStatus = value;
      OnPropertyChanged(nameof(BottomStatus));
    }
  }

  private string? _stopInfo;
  public string? StopInfo {
    get {
      return _stopInfo;
    }
    set {
      _stopInfo = value;
      OnPropertyChanged(nameof(StopInfo));
    }
  }

  #endregion

  private int _currentEntry = 0;
  private BaseVM? _currentEntryVM;
  public int CurrentEntry {
    get { return _currentEntry; }
    set {
      if (value < 0 || value >= Entries.Count) return;
      if (Entries[value] is not EbulaEntryVM now) return;
      if (_currentEntryVM is EbulaEntryVM was) was.IsCurrent = false;
      _currentEntry = value;
      _currentEntryVM = now;
      now.IsCurrent = true;

      if (CurrentEntry < StartEntry) StartEntry = CurrentEntry;
      if (CurrentEntry > StartEntry+9) StartEntry = CurrentEntry;
    }
  }


  private int _startEntry = 0;
  public int StartEntry {
    get { return _startEntry; }
    set {
      _startEntry = Math.Clamp(value, 0, Math.Max(Entries.Count - 15, 0));
      UpdateList();
      OnPropertyChanged(nameof(StartEntry));
    }
  }

  public ObservableCollection<BaseVM> ActiveEntries { get; } = new();
  public ObservableCollection<BaseVM> Entries { get; } = new();

  private Brush? _rowHighlight = null;
  public Brush? RowHighlight {
    get {
      return _rowHighlight;
    }
    set {
      _rowHighlight = value;
      OnPropertyChanged(nameof(RowHighlight));
    }
  }

  #region Footer

  private string? _buttonLabel0 = "Zug";
  public string? ButtonLabel0 {
    get {
      return _buttonLabel0;
    }
    set {
      _buttonLabel0 = value;
      OnPropertyChanged(nameof(ButtonLabel0));
    }
  }

  private string? _buttonLabel1 = "FSD";
  public string? ButtonLabel1 {
    get {
      return _buttonLabel1;
    }
    set {
      _buttonLabel1 = value;
      OnPropertyChanged(nameof(ButtonLabel1));
    }
  }

  private string? _buttonLabel2 = null;
  public string? ButtonLabel2 {
    get {
      return _buttonLabel2;
    }
    set {
      _buttonLabel2 = value;
      OnPropertyChanged(nameof(ButtonLabel2));
    }
  }

  private string? _buttonLabel3 = null;
  public string? ButtonLabel3 {
    get {
      return _buttonLabel3;
    }
    set {
      _buttonLabel3 = value;
      OnPropertyChanged(nameof(ButtonLabel3));
    }
  }

  private string? _buttonLabel4 = "LW";
  public string? ButtonLabel4 {
    get {
      return _buttonLabel4;
    }
    set {
      _buttonLabel4 = value;
      OnPropertyChanged(nameof(ButtonLabel4));
    }
  }

  private string? _buttonLabel5 = "GW";
  public string? ButtonLabel5 {
    get {
      return _buttonLabel5;
    }
    set {
      _buttonLabel5 = value;
      OnPropertyChanged(nameof(ButtonLabel5));
    }
  }

  private string? _buttonLabel6 = "Zeit";
  public string? ButtonLabel6 {
    get {
      return _buttonLabel6;
    }
    set {
      _buttonLabel6 = value;
      OnPropertyChanged(nameof(ButtonLabel6));
    }
  }

  private string? _buttonLabel7 = null;
  public string? ButtonLabel7 {
    get {
      return _buttonLabel7;
    }
    set {
      _buttonLabel7 = value;
      OnPropertyChanged(nameof(ButtonLabel7));
    }
  }

  private string? _buttonLabel8 = null;
  public string? ButtonLabel8 {
    get {
      return _buttonLabel8;
    }
    set {
      _buttonLabel8 = value;
      OnPropertyChanged(nameof(ButtonLabel8));
    }
  }

  private string? _buttonLabel9 = "G";
  public string? ButtonLabel9 {
    get {
      return _buttonLabel9;
    }
    set {
      _buttonLabel9 = value;
      OnPropertyChanged(nameof(ButtonLabel9));
    }
  }


  #endregion
  #endregion
}
