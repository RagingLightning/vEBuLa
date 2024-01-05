using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using vEBuLa.Commands;
using vEBuLa.Extensions;

namespace vEBuLa.ViewModels;
internal class EbulaScreenVM : ScreenBaseVM {
  private ILogger<EbulaEntryVM>? Logger => App.GetService<ILogger<EbulaEntryVM>>();
  private readonly EbulaVM Ebula;

  public EbulaScreenVM(EbulaVM ebula) {
    Ebula = ebula;
    NavigateCommand = new NavigateDefaultScreenC(this);
    AddEntryCommand = new AddEbulaEntryC(ebula);
    EditEntryCommand = new EditEbulaEntryC(this);
    RemoveEntryCommand = new RemoveEbulaEntryC(ebula);

    UpdateEntries();
  }

  public void UpdateEntries() {
    Logger?.LogDebug("Updating EbulaScreen entries, with EditMode={EditMode}", EditMode);
    Entries.Clear();
    EbulaEntryVM? ebulaEntry = null;
    if (EditMode) {
      var preMarker = new EbulaMarkerEntryVM(this, Ebula.Model.Segments[0], EbulaMarkerType.PRE);
      Entries.Add(preMarker);
      Logger?.LogTrace("Added EbulaMarker {Marker} for Segment {Segment} Pre stage; Count={EntryCount}", preMarker, Ebula.Model.Segments[0], Entries.Count);
    }
    Entries.AddRange(Ebula.Model.Segments[0].PreEntries.Select(e => ebulaEntry = new EbulaEntryVM(e, Ebula.Model.ServiceStartTime, ebulaEntry, this)));
    Logger?.LogTrace("Added {AddCount} EbulaEntries from Segment {Segment} Pre stage; Count={EntryCount}", Ebula.Model.Segments[0].Entries.Count, Ebula.Model.Segments[0], Entries.Count);
    foreach (var segment in Ebula.Model.Segments) {
      if (EditMode) {
        var mainMarker = new EbulaMarkerEntryVM(this, segment, EbulaMarkerType.MAIN);
        Entries.Add(mainMarker);
        Logger?.LogTrace("Added EbulaMarker {Marker} for Segment {Segment} Main stage; Count={EntryCount}", mainMarker, segment, Entries.Count);
      }
      Entries.AddRange(segment.Entries.Select(e => ebulaEntry = new EbulaEntryVM(e, Ebula.Model.ServiceStartTime, ebulaEntry, this)));
      Logger?.LogTrace("Added {AddCount} EbulaEntries from Segment {Segment} Main stage; Count={EntryCount}", segment.Entries.Count, segment, Entries.Count);
    }
    if (EditMode) {
      var postMarker = new EbulaMarkerEntryVM(this, Ebula.Model.Segments[^1], EbulaMarkerType.POST);
      Entries.Add(postMarker);
      Logger?.LogTrace("Added EbulaMarker {Marker} for Segment {Segment} Post stage; Count={EntryCount}", postMarker, Ebula.Model.Segments[^1], Entries.Count);
    }
    Entries.AddRange(Ebula.Model.Segments[^1].PostEntries.Select(e => ebulaEntry = new EbulaEntryVM(e, Ebula.Model.ServiceStartTime, ebulaEntry, this)));
    Logger?.LogTrace("Added {AddCount} EbulaEntries from Segment {Segment} Post stage; Count={EntryCount}", Ebula.Model.Segments[^1].Entries.Count, Ebula.Model.Segments[^1], Entries.Count);
    UpdateList();
  }

  public void UpdateList() {
    Logger?.LogDebug("Updating EbulaScreen display list, starting at index {StartIndex}", StartEntry);
    ActiveEntries.Clear();
    ActiveEntries.AddRange(Entries.Skip(StartEntry).Take(15).Reverse());
  }

  private bool _editMode = false;
  public bool EditMode {
    get => _editMode;
    set {
      _editMode = value;
      RowHighlight = value ? new SolidColorBrush(Color.FromRgb(190, 190, 190)) : null;
      UpdateEntries();
    }
  }


  #region Properties

  #region Edit Mode
  public BaseC AddEntryCommand { get; }
  public BaseC EditEntryCommand { get; }
  public BaseC RemoveEntryCommand { get; }
  #endregion

  #region Header

  private int _trainNumber = 0;
  public int TrainNumber {
    get {
      return _trainNumber;
    }
    set {
      _trainNumber = value;
      OnPropertyChanged(nameof(TrainNumber));
    }
  }
  public string FormattedTrainNumber => TrainNumber == 0 ? "000000" : TrainNumber.ToString();

  private string _topStatus = string.Empty;
  public string TopStatus {
    get {
      return _topStatus;
    }
    set {
      _topStatus = value;
      OnPropertyChanged(nameof(TopStatus));
    }
  }

  private DateTime _now = DateTime.Now;
  public DateTime Now {
    get {
      return _now;
    }
    set {
      _now = value;
      OnPropertyChanged(nameof(Now));
      OnPropertyChanged(nameof(Date));
      OnPropertyChanged(nameof(Time));
    }
  }

  public string Date => Now.ToString("dd.MM.yyyy");
  public string Time => Now.ToString("hh:mm:ss");

  private string _speedInfo = string.Empty;
  public string SpeedInfo {
    get {
      return _speedInfo;
    }
    set {
      _speedInfo = value;
      OnPropertyChanged(nameof(SpeedInfo));
    }
  }

  private string _bottomStatus = string.Empty;
  public string BottomStatus {
    get {
      return _bottomStatus;
    }
    set {
      _bottomStatus = value;
      OnPropertyChanged(nameof(BottomStatus));
    }
  }

  private string _stopInfo = string.Empty;
  public string StopInfo {
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
      if (_currentEntryVM is EbulaEntryVM oldEntry)
        oldEntry.IsCurrent = false;
      _currentEntry = value > 0 && value < Entries.Count ? value : 0;
      _currentEntryVM = value > 0 && value < Entries.Count ? Entries[value] : null;
      if (_currentEntryVM is EbulaEntryVM newEntry)
        newEntry.IsCurrent = true;
    }
  }


  private int _startEntry = 0;
  public int StartEntry {
    get { return _startEntry; }
    set {
      _startEntry = Math.Clamp(value, 0, Math.Max(Entries.Count - 15, 0));
      UpdateList();
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

  private string? _buttonLabel0 = null;
  public string? ButtonLabel0 {
    get {
      return _buttonLabel0;
    }
    set {
      _buttonLabel0 = value;
      OnPropertyChanged(nameof(ButtonLabel0));
    }
  }

  private string? _buttonLabel1 = null;
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

  private string? _buttonLabel4 = null;
  public string? ButtonLabel4 {
    get {
      return _buttonLabel4;
    }
    set {
      _buttonLabel4 = value;
      OnPropertyChanged(nameof(ButtonLabel4));
    }
  }

  private string? _buttonLabel5 = null;
  public string? ButtonLabel5 {
    get {
      return _buttonLabel5;
    }
    set {
      _buttonLabel5 = value;
      OnPropertyChanged(nameof(ButtonLabel5));
    }
  }

  private string? _buttonLabel6 = null;
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

  private string? _buttonLabel9 = null;
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

  #region Other

  private bool _active = true;
  public bool Active {
    get {
      return _active;
    }
    set {
      _active = value;
      OnPropertyChanged(nameof(Active));
    }
  }

  #endregion
  #endregion
}
