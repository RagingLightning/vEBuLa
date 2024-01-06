using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using vEBuLa.Commands;
using vEBuLa.Extensions;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;
internal partial class StorageConfigScreenVM : ScreenBaseVM {
  private ILogger<StorageConfigScreenVM>? Logger => App.GetService<ILogger<StorageConfigScreenVM>>();

  public StorageConfigScreenVM(EbulaVM ebula) : base(ebula) {
    ToggleRouteModeCommand = new ToggleCustomRouteC(this);
    LoadConfigCommand = new LoadEbulaConfigC(this);
    SaveConfigCommand = new SaveEbulaConfigC(this);

    EditRouteCommand = new EditPredefinedRouteC(this);

    AddOriginCommand = AddConfigStationC.ORIGIN;
    EditOriginCommand = EditConfigEntryC.ORIGIN;
    RemoveOriginCommand = DeleteConfigStationC.ORIGIN;
    AddSegmentCommand = AddConfigSegmentC.INSTANCE;
    EditSegmentCommand = EditConfigEntryC.SEGMENT;
    RemoveSegmentCommand = DeleteConfigSegmentC.INSTANCE;
    AddDestinationCommand = AddConfigStationC.DESTINATION;
    EditDestinationCommand = EditConfigEntryC.DESTINATION;
    RemoveDestinationCommand = DeleteConfigStationC.DESTINATION;
    SaveRouteCommand = new SaveCustomRouteC(this);

    StartEbulaCommand = new SwitchScreenC(ebula, () => {
      Logger?.LogInformation("Switching to main EBuLa screen");
      Ebula.Model.Segments.Clear();
      if (UsingRoutes) {
        if (SelectedRoute is null) {
          Logger?.LogWarning("No Route is selected. Remaining on Setup screen");
          return null;
        }
        Ebula.Model.Segments.AddRange(SelectedRoute.Model.Segments);
      } else {
        if (CustomRoute.Where(e => e.SelectedSegment is not null).Count() == 0) {
          Logger?.LogWarning("Custom Route contains no Segments. Remaining on Setup screen.");
          return null;
        }
        Ebula.Model.Segments.AddRange(CustomRoute.Where(e => e.SelectedSegment is not null).Select(e => e.SelectedSegment.Model));
      }
      Logger?.LogDebug("EBuLa instance starts with Segments {Segments}", Ebula.Model.Segments);
      return new EbulaScreenVM(ebula);
    });

    Ebula.PropertyChanged += Ebula_PropertyChanged;

    LoadConfig();
  }

  private void Ebula_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
    if (sender != Ebula) return;
    if (e.PropertyName == nameof(Ebula.EditMode)) {
      OnPropertyChanged(nameof(ShowSave));
      OnPropertyChanged(nameof(SelectedRoute));
    }
  }

  public void LoadConfig() {
    if (Ebula.Model.Config is not EbulaConfig config) return;
    Logger?.LogDebug("Loading UI values from EBuLa Config {Config}", Ebula.Model.Config);
    ConfigName = config.Name;

    Routes.Clear();
    Routes.AddRange(config.Routes.Values.Select(r => new EbulaRouteVM(this, r)));
    RouteOverview.Clear();

    CustomRoute.Clear();
    CustomRoute.Add(new EbulaCustomEntryVM(this, config));
  }


  #region Properties
  public BaseC ToggleRouteModeCommand { get; }
  public BaseC EditRouteCommand { get; }
  public BaseC LoadConfigCommand { get; }
  public BaseC SaveConfigCommand { get; }
  public BaseC AddOriginCommand { get; }
  public BaseC EditOriginCommand { get; }
  public BaseC RemoveOriginCommand { get; }
  public BaseC AddSegmentCommand { get; }
  public BaseC EditSegmentCommand { get; }
  public BaseC RemoveSegmentCommand { get; }
  public BaseC AddDestinationCommand { get; }
  public BaseC EditDestinationCommand { get; }
  public BaseC RemoveDestinationCommand { get; }
  public BaseC SaveRouteCommand { get; }

  public BaseC StartEbulaCommand { get; }

  public bool ShowSave => EditMode && UsingCustom;

  private string _configName = string.Empty;
  public string ConfigName {
    get {
      return Ebula.Model.Config?.Name ?? string.Empty;
    }
    set {
      if (Ebula.Model.Config is not null)
        Ebula.Model.Config.Name = value;
      OnPropertyChanged(nameof(ConfigName));
    }
  }

  private bool _usingRoutes = true;
  public bool UsingRoutes {
    get {
      return _usingRoutes;
    }
    set {
      _usingRoutes = value;
      if (value) Logger?.LogDebug("Switching to predefined route view");
      else Logger?.LogDebug("Switching to custom route view");
      OnPropertyChanged(nameof(UsingRoutes));
      OnPropertyChanged(nameof(UsingCustom));
      OnPropertyChanged(nameof(ShowSave));
    }
  }
  public bool UsingCustom {
    get {
      return !_usingRoutes;
    }
    set {
      _usingRoutes = !value;
      if (value) Logger?.LogDebug("Switching to custom route view");
      else Logger?.LogDebug("Switching to predefined route view");
      OnPropertyChanged(nameof(UsingRoutes));
      OnPropertyChanged(nameof(UsingCustom));
      OnPropertyChanged(nameof(ShowSave));
    }
  }

  public ObservableCollection<EbulaRouteVM> Routes { get; } = new();

  private EbulaRouteVM? _selectedRoute;
  public EbulaRouteVM? SelectedRoute {
    get {
      return EditMode ? null : _selectedRoute;
    }
    set {
      _selectedRoute = value;
      RouteOverview.Clear();
      if (value is not null) {
        Logger?.LogInformation("Predefined Route {Route} selected", value);
        RouteOverview.AddRange(value.GenerateOverview());
      }
      if (EditMode)
        EditRouteCommand.Execute(value);
      OnPropertyChanged(nameof(SelectedRoute));
    }
  }

  private string _status = string.Empty;
  public string Status {
    get {
      return _status;
    }
    set {
      _status = value;
      OnPropertyChanged(nameof(Status));
    }
  }

  public ObservableCollection<EbulaRouteEntryVM> RouteOverview { get; } = new();

  public ObservableCollection<EbulaCustomEntryVM> CustomRoute { get; } = new();

  private TimeSpan _departure;
  public TimeSpan Departure {
    get {
      return _departure;
    }
    set {
      _departure = value;
      OnPropertyChanged(nameof(Departure));
      _departureText = value.ToString("hh':'mm':'ss");
      OnPropertyChanged(nameof(DepartureText));
    }
  }
  private string _departureText = "00:00:00";
  public string DepartureText {
    get => _departureText;
    set {
      _departureText = value;
      if (Time().IsMatch(value) && TimeSpan.TryParse(value, out var t)) {
        Logger?.LogInformation("Departure time set to {DepartureTime}", t);
        Departure = t;
      }
    }
  }

  [GeneratedRegex("\\d{2}:\\d{2}:\\d{2}")]
  private static partial Regex Time();


  #endregion
}
