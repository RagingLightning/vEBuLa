using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using vEBuLa.Commands;
using vEBuLa.Events;
using vEBuLa.Extensions;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;
internal class EbulaScreenVM : BaseVM {
  private ILogger<EbulaEntryVM>? Logger => App.GetService<ILogger<EbulaEntryVM>>();
  public override string ToString() => $"EbulaScreen";
  public EbulaVM Ebula { get; }
  private IEbulaGameApi? Api;

  public EbulaScreenVM(EbulaVM ebula) {
    Ebula = ebula;
    ValidRoute = Ebula.Service is EbulaService;
    Ebula.NavigateCommand = new NavigateEbulaScreenC(this);

    AddEntryCommand = AddEbulaEntryC.INSTANCE;
    RemoveEntryCommand = RemoveEbulaEntryC.INSTANCE;

    ebula.PropertyChanged += Ebula_PropertyChanged;

    Api = ebula.GameApi;
    if (Api is not null)
      Api.PositionPassed += GameApi_PositionPassed;

    UpdateEntries();
  }

  public override void Destroy() {
    Ebula.PropertyChanged -= Ebula_PropertyChanged;
    if (Api is not null)
      Api.PositionPassed -= GameApi_PositionPassed;
  }

  public void Ebula_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
    if (sender != Ebula) return;
    switch (e.PropertyName) {
      case nameof(Ebula.EditMode):
        UpdateEntries();
        break;
      case nameof(Ebula.CurrentDate):
        TimeScroll();
        UpdateDelay(true);
        break;
      case nameof(Ebula.GameApi):
        if (Api is not null)
          Api.PositionPassed -= GameApi_PositionPassed;
        Api = Ebula.GameApi;
        if (Api is not null)
          Api.PositionPassed += GameApi_PositionPassed;
        break;
    }
  }

  private void GameApi_PositionPassed(object? sender, PositionPassedEventArgs args) {
    if (Ebula.EditMode) return;
    if (!PositionScrolling) return;
    if (!Entries.Any()) return;

    var entryVM = Entries.OfType<EbulaEntryVM>().FirstOrDefault(vm => vm.Model == args.Entry);

    if (entryVM is null) {
      Logger?.LogWarning("Failed to find view model for {Entry}", args.Entry);
      return;
    }

    Logger?.LogDebug("Scrolling to {Entry}", entryVM);
    CurrentEntry = Entries.IndexOf(entryVM);
  }

  public void UpdateEntries() {
    Logger?.LogDebug("Updating EbulaScreen entries, with EditMode={EditMode}", Ebula.EditMode);
    Entries.Clear();
    if (Ebula.Service is null) return;
    EbulaEntryVM? ebulaEntry = null;
    int ebulaEntryIndex = 0;
    TimeSpan departureOffset = TimeSpan.Zero;

    if (Ebula.RouteEditMode) {
      var preMarker = new EbulaMarkerEntryVM(this, Ebula.Service.Segments[0], EbulaMarkerType.PRE);
      Entries.Add(preMarker);
      Logger?.LogTrace("Added EbulaMarker {Marker} for Segment {Segment} Pre stage; Count={EntryCount}", preMarker, Ebula.Service.Segments[0], Entries.Count);
    }

    foreach (var entry in Ebula.Service.Segments[0].PreEntries) {
      var stop = Ebula.Service.Stops.FirstOrDefault(s => s.EntryLocation == entry.Location && s.EntryName == entry.LocationName);
      Entries.Add(ebulaEntry = new EbulaEntryVM(entry, ebulaEntry, stop, this));
    }
    Logger?.LogTrace("Added {AddCount} EbulaEntries from Segment {Segment} Pre stage; Count={EntryCount}", Ebula.Service.Segments[0].Entries.Count, Ebula.Service.Segments[0], Entries.Count);

    foreach (var segment in Ebula.Service.Segments) {
      if (Ebula.RouteEditMode) {
        var mainMarker = new EbulaMarkerEntryVM(this, segment, EbulaMarkerType.MAIN);
        Entries.Add(mainMarker);
        Logger?.LogTrace("Added EbulaMarker {Marker} for Segment {Segment} Main stage; Count={EntryCount}", mainMarker, segment, Entries.Count);
      }

      foreach (var entry in segment.Entries) {
        var stop = Ebula.Service.Stops.FirstOrDefault(s => s.EntryLocation == entry.Location && s.EntryName == entry.LocationName);
        Entries.Add(ebulaEntry = new EbulaEntryVM(entry, ebulaEntry, stop, this));
      }
      departureOffset += segment.Duration;
      Logger?.LogTrace("Added {AddCount} EbulaEntries from Segment {Segment} Main stage; Count={EntryCount}", segment.Entries.Count, segment, Entries.Count);
    }

    if (Ebula.RouteEditMode) {
      var postMarker = new EbulaMarkerEntryVM(this, Ebula.Service.Segments[^1], EbulaMarkerType.POST);
      Entries.Add(postMarker);
      Logger?.LogTrace("Added EbulaMarker {Marker} for Segment {Segment} Post stage; Count={EntryCount}", postMarker, Ebula.Service.Segments[^1], Entries.Count);
    }

    foreach (var entry in Ebula.Service.Segments[^1].PostEntries) {
      var stop = Ebula.Service.Stops.FirstOrDefault(s => s.EntryLocation == entry.Location && s.EntryName == entry.LocationName);
      Entries.Add(ebulaEntry = new EbulaEntryVM(entry, ebulaEntry, stop, this));
    }
    Logger?.LogTrace("Added {AddCount} EbulaEntries from Segment {Segment} Post stage; Count={EntryCount}", Ebula.Service.Segments[^1].Entries.Count, Ebula.Service.Segments[^1], Entries.Count);

    if (!Ebula.EditMode)
      CurrentEntry = 0;
    UpdateList();
  }

  public void UpdateList() {
    Logger?.LogDebug("Updating EbulaScreen display list, starting at index {StartIndex}", StartEntry);
    ActiveEntries.Clear();

    foreach (var entry in Entries) {
      if (entry is not EbulaEntryVM ee) continue;
      ee.ForceSpeedDisplay = false;
      ee.NoSpeedLine = false;
    }
    var displayUnits = 0;
    if (Ebula.NormalMode)
      ActiveEntries.AddRange(Entries.Skip(StartEntry).TakeWhile(e => {
        if (e is not EbulaEntryVM ee)
          displayUnits += 1;
        else
          displayUnits += ee.DisplayUnits;
        return displayUnits <= 15;
      }).Reverse());
    else
      ActiveEntries.AddRange(Entries.Reverse());

    if (ActiveEntries[^1] is EbulaEntryVM ae) {
      ae.ForceSpeedDisplay = true;
      ae.NoSpeedLine = true;
    }

    CreatePreviewInfo();
  }

  private void CreatePreviewInfo() {
    if (Entries.Count <= StartEntry + 16) {
      SpeedInfo = null;
      StopInfo = null;
    }
    bool speedFound = false, stopFound = false;
    foreach (var entry in Entries.Skip(StartEntry + 15)) {
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

  private void TimeScroll() {
    if (Ebula.EditMode) return;
    if (!TimeScrolling) return;
    if (Entries.Count == 0 || CurrentEntry == 0 || TimerJumpTarget == 0) return;
    if (Entries[CurrentEntry] is not EbulaEntryVM ce) return;
    if (Entries[TimerJumpTarget] is not EbulaEntryVM te) {
      FindNextTarget();
      return;
    }

    Logger?.LogTrace("Processing time-based scrolling: {CurrentEntry} > {TargetEntry}, +{Delay}", ce, te, Ebula.ServiceDelay.TotalMinutes);
    if (ce.Arrival is not null && ce.Departure is not null && Ebula.CurrentDate - ce.Departure > TimeSpan.FromSeconds(15) + Ebula.ServiceDelay) {
      Logger?.LogDebug("Departure Time of {CurrentEntry} passed", ce);
      CurrentEntry++;
    }
    else if (te.Arrival is not null && Ebula.CurrentDate - te.Arrival > TimeSpan.FromSeconds(2) + Ebula.ServiceDelay) {
      Logger?.LogDebug("Arrival Time of {TargetEntry} reached, +{Delay}", te, Ebula.ServiceDelay.TotalMinutes);
      CurrentEntry = TimerJumpTarget;
    }
    else if (te.Arrival is null && te.Departure is not null && Ebula.CurrentDate - te.Departure < TimeSpan.FromSeconds(2) + Ebula.ServiceDelay) {
      Logger?.LogDebug("Passing Time of {CurrentEntry} reached, +{Delay}", te, Ebula.ServiceDelay.TotalMinutes);
      CurrentEntry = TimerJumpTarget;
    }
  }

  public void FindNextTarget() {
    for (int i = CurrentEntry + 1; i < Entries.Count; i++) {
      if (Entries[i] is not EbulaEntryVM ee) continue;
      TimerJumpTarget = i;
      if (ee.Departure is not null && ee.Departure + Ebula.ServiceDelay > Ebula.CurrentDate) break;
      if (ee.Arrival is not null && ee.Arrival + Ebula.ServiceDelay > Ebula.CurrentDate) break;
    }
  }

  internal void UpdateDelay(bool time) {
    if (Ebula.EditMode) return;
    if (TimeScrolling && time) return;
    if (!Entries.Any()) return;

    if (RelevantTimeFrame is null || CurrentEntry >= RelevantTimeFrame?.endIndex || CurrentEntry < RelevantTimeFrame?.startIndex)
      RelevantTimeFrame = GetRelevantTimeFrame(CurrentEntry);

    if (RelevantTimeFrame is not (int start, int end))
      return;

    var startEntry = Entries[start] as EbulaEntryVM;
    var endEntry = Entries[end] as EbulaEntryVM;

    if (start != CurrentEntry
      && Ebula.GameApi?.GetPosition() is Vector2 currentPos
      && startEntry?.GpsLocation is Vector2 startPos
      && endEntry?.GpsLocation is Vector2 endPos) { // In transit between stations with GPS

      if (startEntry?.Departure is not DateTime startTime)
        return;

      if ((endEntry?.Arrival ?? endEntry?.Departure) is not DateTime endTime)
        return;

      var scheduledTime = MathEx.ExtrapolateFromTimeFrame(startPos, startTime, endPos, endTime, currentPos);
      var delay = Ebula.CurrentDate - scheduledTime;
      Ebula.ServiceDelay = new TimeSpan(delay.Hours, delay.Minutes, delay.Seconds);
    }
    else { // In transit without GPS or stopped at station
      if (startEntry?.Departure is not DateTime scheduleTime)
        return;

      var delay = Ebula.CurrentDate - scheduleTime;
      Ebula.ServiceDelay = new TimeSpan(delay.Hours, delay.Minutes, delay.Seconds);
    }
    //Ebula.StopServiceClock();
    OnPropertyChanged(nameof(ServiceDelay));
    OnPropertyChanged(nameof(HasDelay));
    OnPropertyChanged(nameof(PositiveDelay));
    //Ebula.RunServiceClock();
  }

  private (int startIndex, int endIndex)? GetRelevantTimeFrame(int entryIndex) {
    if (!Entries.Any()) return null;

    var fromIndex = -1;
    for (int i = entryIndex; i >= 0; i--) {
      if (Entries[i] is not EbulaEntryVM ee) continue;
      if (ee.Arrival is null && ee.Departure is null) continue;
      fromIndex = i;
      break;
    }

    var toIndex = Entries.Count + 1;
    for (int i = entryIndex + 1; i < Entries.Count; i++) {
      if (Entries[i] is not EbulaEntryVM ee) continue;
      if (ee.Arrival is null && ee.Departure is null) continue;
      toIndex = i;
      break;
    }

    if (fromIndex < 0 || toIndex > Entries.Count)
      return null;

    return (fromIndex, toIndex);
  }

  #region Properties

  #region Edit Mode
  public BaseC AddEntryCommand { get; }
  public BaseC RemoveEntryCommand { get; }
  #endregion

  #region Popup Windows

  private BaseVM? _popupWindow;
  public BaseVM? PopupWindow {
    get {
      return _popupWindow;
    }
    set {
      _popupWindow = value;
      if (value is null)
        Ebula.NavigateCommand = new NavigateEbulaScreenC(this);
      OnPropertyChanged(nameof(PopupWindow));
    }
  }

  #endregion

  #region Settings

  private bool _scrollBackwards;
  public bool ScrollBackwards {
    get {
      return _scrollBackwards;
    }
    set {
      _scrollBackwards = value;
      if (value)
        Logger?.LogDebug("Scrolling {Direction}", "backwards");
      else
        Logger?.LogDebug("Scrolling {Direction}", "forwards");
      OnPropertyChanged(nameof(ScrollBackwards));
    }
  }
  public int ServiceDelay {
    get {
      return (int) Math.Floor(Ebula.ServiceDelay.TotalMinutes);
    }
    set {
      Ebula.ServiceDelay = TimeSpan.FromMinutes(value);
      Logger?.LogDebug("Service Delay {Minutes} min", value);
      OnPropertyChanged(nameof(ServiceDelay));
      OnPropertyChanged(nameof(HasDelay));
      OnPropertyChanged(nameof(PositiveDelay));
    }
  }
  public bool HasDelay => ServiceDelay != 0;
  public bool PositiveDelay => ServiceDelay > 0;
  public (int startIndex, int endIndex)? RelevantTimeFrame { get; private set; }

  private EbulaScrollMode _scrollMode = EbulaScrollMode.POSITION;
  public EbulaScrollMode ScrollMode {
    get {
      return _scrollMode;
    }
    set {
      _scrollMode = value;

      Logger?.LogDebug("{ScrollMode} Scrolling Mode", value switch {
        EbulaScrollMode.POSITION => "POSITION",
        EbulaScrollMode.TIME => "TIME-BASED",
        EbulaScrollMode.MANUAL => "MANUAL",
        _ => "UNKNOWN"
      });

      OnPropertyChanged(nameof(ScrollMode));
      OnPropertyChanged(nameof(TimeScrolling));
      OnPropertyChanged(nameof(ManualScrolling));
    }
  }

  public bool PositionScrolling => _scrollMode == EbulaScrollMode.POSITION;
  public bool TimeScrolling => _scrollMode == EbulaScrollMode.TIME;
  public bool ManualScrolling => _scrollMode == EbulaScrollMode.MANUAL;

  #endregion

  #region Header

  private bool _validRoute = false;
  public bool ValidRoute {
    get {
      return _validRoute;
    }
    set {
      _validRoute = value;
      OnPropertyChanged(nameof(ValidRoute));
      OnPropertyChanged(nameof(InvalidRoute));
    }
  }
  public bool InvalidRoute => !ValidRoute;

  private string? _speedInfo = null;
  public string? SpeedInfo {
    get {
      return _speedInfo;
    }
    set {
      _speedInfo = value;
      OnPropertyChanged(nameof(SpeedInfo));
    }
  }

  private string? _bottomStatus = null; //unused
  public string? BottomStatus {
    get {
      return _bottomStatus;
    }
    set {
      _bottomStatus = value;
      OnPropertyChanged(nameof(BottomStatus));
    }
  }

  private string? _stopInfo = null;
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
  private int TimerJumpTarget = -1;

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
      if (CurrentEntry > StartEntry + 9) StartEntry = CurrentEntry;
      UpdateDelay(false);
      FindNextTarget();
    }
  }

  private int _startEntry = 0;
  public int StartEntry {
    get { return _startEntry; }
    set {
      _startEntry = Math.Clamp(value, 0, Math.Max(Entries.Count - 15, 0));
      Application.Current.Dispatcher.Invoke(UpdateList);
      OnPropertyChanged(nameof(StartEntry));
    }
  }

  public BaseVM? SelectedEntry { get; set; } = null;
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

  #endregion
}
