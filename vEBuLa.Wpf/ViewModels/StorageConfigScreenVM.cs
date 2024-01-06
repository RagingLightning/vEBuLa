using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vEBuLa.Commands;
using vEBuLa.Extensions;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;
internal partial class StorageConfigScreenVM : ScreenBaseVM {

  public StorageConfigScreenVM(EbulaVM ebula) : base(ebula) {
    ToggleRouteModeCommand = new ToggleCustomRouteC(this);
    LoadConfigCommand = new LoadEbulaConfigC(this);
    //SaveConfigCommand;

    AddOriginCommand = AddConfigStationC.ORIGIN;
    EditOriginCommand = EditConfigEntryC.ORIGIN;
    RemoveOriginCommand = DeleteConfigStationC.ORIGIN;
    AddSegmentCommand = AddConfigSegmentC.INSTANCE;
    EditSegmentCommand = EditConfigEntryC.SEGMENT;
    RemoveSegmentCommand = DeleteConfigSegmentC.INSTANCE;
    AddDestinationCommand = AddConfigStationC.DESTINATION;
    EditDestinationCommand = EditConfigEntryC.DESTINATION;
    RemoveDestinationCommand = DeleteConfigStationC.DESTINATION;

    StartEbulaCommand = new SwitchScreenC(ebula, () => {
      Ebula.Model.Segments.Clear();
      if (UsingRoutes) {
        if (SelectedRoute is null) return null;
        Ebula.Model.Segments.AddRange(SelectedRoute.Model.Segments);
      } else {
        if (CustomRoute.Where(e => e.SelectedSegment is not null).Count() == 0) return null;
        Ebula.Model.Segments.AddRange(CustomRoute.Where(e => e.SelectedSegment is not null).Select(e => e.SelectedSegment.Model));
      }
      return new EbulaScreenVM(ebula);
    });

    LoadConfig();
  }

  public void LoadConfig() {
    if (Ebula.Model.Config is not EbulaConfig config) return;
    ConfigName = config.Name;

    Routes.Clear();
    Routes.AddRange(config.Routes.Values.Select(r => new EbulaRouteVM(this, r)));

    CustomRoute.Clear();
    CustomRoute.Add(new EbulaCustomEntryVM(this, config));
  }


  #region Properties
  public BaseC ToggleRouteModeCommand { get; }
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

  public BaseC StartEbulaCommand { get; }

  private string _configName = string.Empty;
  public string ConfigName {
    get {
      return _configName;
    }
    set {
      _configName = value;
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
      OnPropertyChanged(nameof(UsingRoutes));
      OnPropertyChanged(nameof(UsingCustom));
    }
  }
  public bool UsingCustom {
    get {
      return !_usingRoutes;
    }
    set {
      _usingRoutes = !value;
      OnPropertyChanged(nameof(UsingRoutes));
      OnPropertyChanged(nameof(UsingCustom));
    }
  }

  public ObservableCollection<EbulaRouteVM> Routes { get; } = new();

  private EbulaRouteVM? _selectedRoute;
  public EbulaRouteVM? SelectedRoute {
    get {
      return _selectedRoute;
    }
    set {
      _selectedRoute = value;
      RouteOverview.Clear();
      if (value is not null)
        RouteOverview.AddRange(value.GenerateOverview());
      OnPropertyChanged(nameof(SelectedRoute));
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
      if (Time().IsMatch(value) && TimeSpan.TryParse(value, out var t))
        Departure = t;
    }
  }

  [GeneratedRegex("\\d{2}:\\d{2}:\\d{2}")]
  private static partial Regex Time();


  #endregion
}
