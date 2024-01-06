using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using vEBuLa.Commands;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;
internal class EbulaVM : BaseVM {
  private ILogger<EbulaEntryVM>? Logger => App.GetService<ILogger<EbulaEntryVM>>();
  public Ebula Model { get; set; }

  public EbulaVM() {
    Model = new Ebula(null);
    Screen = new StorageConfigScreenVM(this);
    ToggleScreenCommand = new ToggleScreenC(this);
    ToggleEditCommand = new ToggleEditModeC(this);

    Screen.PropertyChanged += Screen_NavigateCommandChanged;
  }

  // Propagate change of Navigation command
  private void Screen_NavigateCommandChanged(object? sender, PropertyChangedEventArgs e) {
    if (sender != Screen) return;
    OnPropertyChanged(nameof(NavigateCommand));
  }

  #region Properties

  private ScreenBaseVM _screen;
  public ScreenBaseVM Screen {
    get {
      return _screen;
    }
    set {
      if (_screen is not null)
        _screen.PropertyChanged -= Screen_NavigateCommandChanged;
      _screen = value;
      value.PropertyChanged += Screen_NavigateCommandChanged;
      OnPropertyChanged(nameof(Screen));
      OnPropertyChanged(nameof(NavigateCommand));
    }
  }

  private bool _active;
  public bool Active {
    get {
      return _active;
    }
    set {
      _active = value;
      OnPropertyChanged(nameof(Active));
    }
  }

  private bool _editMode;
  public bool EditMode {
    get {
      return _editMode;
    }
    set {
      _editMode = value;
      OnPropertyChanged(nameof(EditMode));
      OnPropertyChanged(nameof(NormalMode));
    }
  }
  public bool NormalMode => !EditMode;

  #region Commands

  public BaseC ToggleEditCommand { get; }
  public BaseC ToggleScreenCommand { get; }
  public BaseC NavigateCommand => Screen.NavigateCommand; //managed by the screen instance

  private BaseC _buttonStCommand;
  public BaseC ButtonStCommand {
    get {
      return _buttonStCommand;
    }
    set {
      _buttonStCommand = value;
      OnPropertyChanged(nameof(ButtonStCommand));
    }
  }
  #endregion

  #endregion

}
