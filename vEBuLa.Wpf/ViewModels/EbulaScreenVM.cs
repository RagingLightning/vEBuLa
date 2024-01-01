using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.Wpf.Commands;
using vEBuLa.Wpf.Extensions;

namespace vEBuLa.Wpf.ViewModels;
public class EbulaScreenVM : ScreenBaseVM {

  public void UpdateList() {
    StartEntry = StartEntry; // call StartEntry.set
  }

  #region Properties
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
      ActiveEntries.Clear();
      _startEntry = Entries.Count < value + 15 ? Entries.Count - 15 : value;
      ActiveEntries.AddRange(Entries.Skip(_startEntry).Take(15).Reverse());
    }
  }

  public ObservableCollection<EbulaEntryVM> ActiveEntries { get; } = new();
  public ObservableCollection<EbulaEntryVM> Entries { get; } = new();

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
  #endregion
}
