using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Timers;
using vEBuLa.Commands;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;
internal partial class EbulaVM : BaseVM {
  private ILogger<EbulaEntryVM>? Logger => App.GetService<ILogger<EbulaEntryVM>>();
  public Ebula Model { get; set; }
  public GlobalHotkeyHelper? Hotkeys { get; private set; }

  public EbulaVM() {
    Logger?.LogInformation("Loading EBuLa System");
    Model = new Ebula(null);
    Logger?.LogDebug("Screen loading");
    Screen = new SetupScreenVM(this);
    ToggleScreenCommand = new ToggleScreenC(this);
    ToggleEditCommand = new ToggleEditModeC(this);
    ExitAppCommand = new ExitAppC();
  }

  public void SetHotkeys() {
    //https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
    Hotkeys = new GlobalHotkeyHelper();
    Logger?.LogInformation("Registering global hotkeys");
    Hotkeys.RegisterHotKey(() => NavigateCommand?.Execute(NavAction.ACCEPT), 0x6B); // VK_ADD - Numpad Add
    Hotkeys.RegisterHotKey(() => NavigateCommand?.Execute(NavAction.MOVE_DOWN), 0x6F); // VK_DIVIDE - Numpad Div
    Hotkeys.RegisterHotKey(() => NavigateCommand?.Execute(NavAction.MOVE_UP), 0x6A); // VK_MULTIPLY - Numpad Mul
    Hotkeys.RegisterHotKey(() => NavigateCommand?.Execute(NavAction.MOVE_RIGHT), 0x6D); // VK_SUBTRACT - Numpad Sub
  }

  public void UnsetHotkeys() {
    Logger?.LogInformation("Unregistering global hotkeys");
    Hotkeys = null;
  }

  public void RunServiceClock() {
    if (ServiceClock is null) {
      ServiceClock = new Timer(TimeSpan.FromSeconds(0.5));
      ServiceClock.AutoReset = true;
      ServiceClock.Elapsed += (sender, e) => {
        ServiceTime += e.SignalTime - LastTimeUpdate;
        LastTimeUpdate = e.SignalTime;
      };
    }
    LastTimeUpdate = DateTime.Now;
    ServiceClock.Start();
  }

  public void StopServiceClock() {
    if (ServiceClock is not null)
      ServiceClock.Stop();
  }

  public void MarkDirty() {
    if (Model.Config.IsDirty) return;
    Model.Config.MarkDirty();
    ConfigDirtyChanged?.Invoke();
  }

  public void MarkClean() {
    if (Model.Config.IsDirty) return;
    ConfigDirtyChanged?.Invoke();
  }

  public event Action ConfigDirtyChanged;

  #region Properties
  private Timer? ServiceClock = null;
  private DateTime LastTimeUpdate = DateTime.MinValue;
  private TimeSpan _serviceStartTime = TimeSpan.Zero;
  public TimeSpan ServiceStartTime {
    get => _serviceStartTime;
    set {
      _serviceStartTime = value;
      OnPropertyChanged(nameof(ServiceStartTime));
      OnPropertyChanged(nameof(FormattedServiceStartTime));
    }
  }
  public string FormattedServiceStartTime {
    get => ServiceStartTime.ToString("hh':'mm':'ss");
    set {
      if (ShortTime().IsMatch(value) && TimeSpan.TryParse($"{value[..2]}:{value[2..4]}:{value[4..]}", out var t)
        || Time().IsMatch(value) && TimeSpan.TryParse(value, out t)) {
        ServiceStartTime = t;
      }
    }
  }
  public TimeSpan ServiceDelay { get; set; } = TimeSpan.Zero;
  public TimeSpan ServiceElapsedTime { get; set; } = TimeSpan.Zero;
  public TimeSpan ServiceTime {
    get {
      return ServiceStartTime + ServiceElapsedTime;
    }
    set {
      ServiceElapsedTime = value - ServiceStartTime;
      OnPropertyChanged(nameof(ServiceTime));
      OnPropertyChanged(nameof(FormattedServiceTime));
    }
  }
  public string FormattedServiceTime => ServiceTime.ToString(@"hh\:mm\:ss");

  private string _serviceName = "000000";
  public string ServiceName {
    get {
      return _serviceName;
    }
    set {
      _serviceName = value;
      OnPropertyChanged(nameof(ServiceName));
    }
  }

  private BaseVM _screen;
  public BaseVM Screen {
    get {
      return _screen;
    }
    set {
      _screen = value;
      OnPropertyChanged(nameof(Screen));
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
  public BaseC ExitAppCommand { get; }

  private BaseC? _navigateCommand;
  public BaseC? NavigateCommand {
    get {
      return _navigateCommand;
    }
    set {
      _navigateCommand = value;
      OnPropertyChanged(nameof(NavigateCommand));
    }
  }
  #endregion

  #endregion

  [GeneratedRegex("\\d{2}:\\d{2}:\\d{2}")]
  private static partial Regex Time();

  [GeneratedRegex("\\d{6}")]
  private static partial Regex ShortTime();

}
