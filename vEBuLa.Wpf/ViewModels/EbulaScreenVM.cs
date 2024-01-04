using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using vEBuLa.Commands;
using vEBuLa.Extensions;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;
internal class EbulaScreenVM : ScreenBaseVM {
  private ILogger<EbulaScreenVM>? Logger => App.AppHost?.Services.GetRequiredService<ILogger<EbulaScreenVM>>();
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
    Entries.Clear();
    if (EditMode) Entries.Add(EbulaEntryVM.EditEntry);
    EbulaEntryVM? ebulaEntry = null;
    foreach (var entry in Ebula.Model.Entries) {
      ebulaEntry = new EbulaEntryVM(entry, new TimeSpan(0, 0, 0), ebulaEntry);
      Entries.Add(ebulaEntry);
    }
    UpdateList();
  }

  public void UpdateList() {
    ActiveEntries.Clear();
    ActiveEntries.AddRange(Entries.Skip(StartEntry).Take(15).Reverse());
  }

  private void EntrySelected(EbulaEntryVM entry) {
  }

  private bool _editMode = false;
  public bool EditMode {
    get => _editMode;
    set {
      _editMode = value;
      RowHighlight = value ? new SolidColorBrush(Color.FromRgb(190, 190, 190)) : null;

      if (value && Entries[0] != EbulaEntryVM.EditEntry) {
        Entries.Insert(0, EbulaEntryVM.EditEntry);
        StartEntry += StartEntry == 0 ? 0 : 1;
      }
      else if (!value && Entries[0] == EbulaEntryVM.EditEntry) {
        Entries.RemoveAt(0);
        StartEntry -= StartEntry == 0 ? 0 : 1;
      }
    }
  }


  #region Properties

  #region Edit Mode
  private EbulaEntryVM? _selectedEntry;
  public EbulaEntryVM? SelectedEntry {
    get {
      return _selectedEntry;
    }
    set {
      _selectedEntry = value;
      if (EditMode && value is not null) EntrySelected(value);
      else _selectedEntry = null;
      OnPropertyChanged(nameof(SelectedEntry));
    }
  }

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
  private EbulaEntryVM? _currentEntryVM;
  public int CurrentEntry {
    get { return _currentEntry; }
    set {
      if (_currentEntryVM is not null)
        _currentEntryVM.IsCurrent = false;
      _currentEntry = value > 0 && value < Entries.Count ? value : 0;
      _currentEntryVM = value > 0 && value < Entries.Count ? Entries[value] : null;
      if (_currentEntryVM is not null)
        _currentEntryVM.IsCurrent = true;
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

  public ObservableCollection<EbulaEntryVM> ActiveEntries { get; } = new();
  public ObservableCollection<EbulaEntryVM> Entries { get; } = new();

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
