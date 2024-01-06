using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using vEBuLa.Commands;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;
internal class EbulaVM : BaseVM {
  private ILogger<EbulaEntryVM>? Logger => App.GetService<ILogger<EbulaEntryVM>>();
  public Ebula Model { get; set; }
  public GlobalHotkeyHelper? Hotkeys { get; private set; }

  public EbulaVM() {
    Logger?.LogInformation("Loading EBuLa System");
    Model = new Ebula(null);
    Logger?.LogDebug("Screen loading");
    Screen = new StorageConfigScreenVM(this);
    ToggleScreenCommand = new ToggleScreenC(this);
    ToggleEditCommand = new ToggleEditModeC(this);

    Screen.PropertyChanged += Screen_NavigateCommandChanged;
  }

  public void SetHotkeys() {
    //https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
    Hotkeys = new GlobalHotkeyHelper();
    Logger?.LogInformation("Registering global hotkeys");
    Hotkeys.RegisterHotKey(() => NavigateCommand.Execute(NavAction.ACCEPT), 0x6B); // VK_ADD - Numpad Add
    Hotkeys.RegisterHotKey(() => NavigateCommand.Execute(NavAction.MOVE_DOWN), 0x6F); // VK_DIVIDE - Numpad Div
    Hotkeys.RegisterHotKey(() => NavigateCommand.Execute(NavAction.MOVE_UP), 0x6A); // VK_MULTIPLY - Numpad Mul
    Hotkeys.RegisterHotKey(() => NavigateCommand.Execute(NavAction.MOVE_RIGHT), 0x6D); // VK_SUBTRACT - Numpad Sub
  }

  public void UnsetHotkeys() {
    Logger?.LogInformation("Unregistering global hotkeys");
    Hotkeys = null;
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
