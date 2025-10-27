using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows;
using vEBuLa.Commands;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;

public class EbulaVM : BaseVM {
  private ILogger<EbulaEntryVM>? Logger => App.GetService<ILogger<EbulaEntryVM>>();
  public Ebula Model { get; set; }

  private GlobalHotkeyHelper? _hotkeys;
  public bool Hotkeys => _hotkeys is not null && _hotkeys.Count > 0;

  public EbulaVM(ProgramConfig config) {
    Logger?.LogInformation("Loading EBuLa System");
    
    Model = new Ebula(App.ConfigFolder, false);

    if (!string.IsNullOrWhiteSpace(config.ApiKey_TSW6))
      GameApi = new EbulaTswApi(config.ApiKey_TSW6);

    RunServiceClock();

    Logger?.LogDebug("Screen loading");
    _screen = new EbulaScreenVM(this);
    ToggleScreenCommand = new ToggleScreenC(this);
    ToggleEditCommand = new ToggleEditModeC(this);
    ExitAppCommand = new ExitAppC();
  }

  public void SetHotkeys() {
    //https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
    Logger?.LogInformation("Registering global hotkeys");
    _hotkeys ??= new GlobalHotkeyHelper();

    _hotkeys.RegisterHotKey(() => NavigateCommand?.Execute(NavAction.ACCEPT), 0x6B); // VK_ADD - Numpad Add
    _hotkeys.RegisterHotKey(() => NavigateCommand?.Execute(NavAction.MOVE_DOWN), 0x6F); // VK_DIVIDE - Numpad Div
    _hotkeys.RegisterHotKey(() => NavigateCommand?.Execute(NavAction.MOVE_UP), 0x6A); // VK_MULTIPLY - Numpad Mul
    _hotkeys.RegisterHotKey(() => NavigateCommand?.Execute(NavAction.MOVE_RIGHT), 0x6D); // VK_SUBTRACT - Numpad Sub
    OnPropertyChanged(nameof(Hotkeys));
  }

  public void UnsetHotkeys() {
    Logger?.LogInformation("Unregistering global hotkeys");
    if (_hotkeys is null) return;

    _hotkeys.UnregisterHotKeys();
    OnPropertyChanged(nameof(Hotkeys));
  }

  internal void LoadService(IEbulaService service) {
    Logger?.LogDebug("Loading Ebula for {Service}", service);

    Service = service;
    ServiceDelay = TimeSpan.Zero;

    if (GameApi?.IsAvailable == true && GameApi.GetGameTime() is DateTime gameTime) {
      ServiceStartDate = gameTime.Date + service.StartTime;
    }
    else {
      ServiceStartDate = DateTime.Today + service.StartTime;
    }

    ServiceEntries.Clear();
    foreach (var segment in service.Segments) {
      foreach (var entry in segment.Entries) {
        if (entry.GpsLocation is not Vector2 pos)
          continue;

        ServiceEntries.Add(entry);
      }
    }
    GameApi?.MonitorPositions(ServiceEntries);

    Screen.Destroy();
    Screen = new LoadingScreenVM();

    NavigateCommand = null;

    new Thread(() => {
      Thread.Sleep(3500);
      Application.Current.Dispatcher.Invoke(() => {
        Screen = new EbulaScreenVM(this);
        RunServiceClock();
      });
    }).Start();
  }

  internal void LoadSegments(IEnumerable<EbulaSegment> segments, string serviceName, TimeSpan serviceStartTime) {
    Logger?.LogDebug("Loading Ebula with {SegmentCount} segments", segments.Count());

    LoadService(new EbulaCustomService(segments, serviceStartTime, serviceName));
  }

  public void RunServiceClock() {
    if (ServiceClock is null) {
      ServiceClock = new System.Timers.Timer(TimeSpan.FromSeconds(0.5));
      ServiceClock.AutoReset = true;
      ServiceClock.Elapsed += (sender, e) => {
        if (TimerTicks == 0 && GameApi?.IsAvailable == true && GameApi.GetGameTime() is DateTime gameTime) {
          CurrentDate = gameTime;
        }
        else {
          CurrentDate += e.SignalTime - LastTimeUpdate;
          TimerTicks = (TimerTicks + 1) % 20;
        }
        LastTimeUpdate = e.SignalTime;

        var serviceDate = DateOnly.FromDateTime(ServiceStartDate);
        var currentDate = DateOnly.FromDateTime(CurrentDate);

        if (serviceDate != currentDate)
          ServiceStartDate = new DateTime(currentDate, TimeOnly.FromDateTime(ServiceStartDate));
      };
    }

    if (Service is not null)
      CurrentDate = ServiceStartDate;

    LastTimeUpdate = DateTime.Now;
    ServiceClock.Start();
  }

  public void StopServiceClock() {
    if (ServiceClock is not null) {
      ServiceClock.AutoReset = false;
      ServiceClock.Stop();
    }
  }

  public void MarkDirty() {
    if (Model.Config is not EbulaConfig cfg || cfg.IsDirty) return;
    cfg.MarkDirty();
    ConfigDirtyChanged?.Invoke();
  }

  public void MarkClean() {
    if (Model.Config is not EbulaConfig cfg || cfg.IsDirty) return;
    ConfigDirtyChanged?.Invoke();
  }

  public event Action? ConfigDirtyChanged;

  #region Properties

  private System.Timers.Timer? ServiceClock = null;
  private int TimerTicks = 0;
  private DateTime LastTimeUpdate = DateTime.MinValue;

  public ObservableCollection<EbulaSegmentVM> Segments { get; } = new();

  public IEbulaService? Service { get; private set; }
  private List<EbulaEntry> ServiceEntries { get; } = [];
  public TimeSpan ServiceDelay { get; set; } = TimeSpan.Zero;
  public DateTime ServiceStartDate { get; private set; } = DateTime.Today;

  private DateTime _currentDate = DateTime.Now;
  public DateTime CurrentDate {
    get => _currentDate;
    set {
      _currentDate = value;
      OnPropertyChanged(nameof(CurrentDate));
      OnPropertyChanged(nameof(FormattedDate));
      OnPropertyChanged(nameof(FormattedTime));
    }
  }

  public string FormattedDate => CurrentDate.ToString(@"dd\.MM\.yyyy", CultureInfo.InvariantCulture);
  public string FormattedTime => CurrentDate.ToString(@"HH\:mm\:ss", CultureInfo.InvariantCulture);

  private IEbulaGameApi? _gameApi;
  public IEbulaGameApi? GameApi {
    get {
      return _gameApi;
    }
    set {
      _gameApi = value;

      if (value is not null && ServiceEntries.Count > 0)
        value.MonitorPositions(ServiceEntries);
      OnPropertyChanged(nameof(GameApi));
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
  private bool _editMode = false;
  public bool EditMode {
    get {
      return _editMode;
    }
    set {
      _editMode = value;
      OnPropertyChanged(nameof(EditMode));
      OnPropertyChanged(nameof(NormalMode));
      OnPropertyChanged(nameof(ServiceEditMode));
      OnPropertyChanged(nameof(RouteEditMode));
    }
  }
  public bool NormalMode => !EditMode;

  private bool _serviceEditMode;
  public bool ServiceEditMode {
    get {
      return _serviceEditMode && _editMode;
    }
    set {
      _serviceEditMode = value;
      OnPropertyChanged(nameof(ServiceEditMode));
      OnPropertyChanged(nameof(RouteEditMode));
    }
  }

  public bool RouteEditMode {
    get => !_serviceEditMode && _editMode;
    set => ServiceEditMode = !value;
  }

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
}
