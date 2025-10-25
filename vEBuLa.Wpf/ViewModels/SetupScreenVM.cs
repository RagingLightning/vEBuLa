using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using vEBuLa.Commands;
using vEBuLa.Commands.Setup;
using vEBuLa.Extensions;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;

public class SetupScreenVM : BaseVM {
  private ILogger<SetupScreenVM>? Logger => App.GetService<ILogger<SetupScreenVM>>();
  public EbulaVM Ebula { get; }

  public SetupScreenVM(EbulaVM ebula) {
    Ebula = ebula;
    Ebula.NavigateCommand = new NavigateSetupScreenC(this);

    EditRouteCommand = new EditPredefinedRouteC(this);
    EditServiceCommand = new EditServiceC(this);

    AddOriginCommand = AddConfigStationC.ORIGIN;
    EditOriginCommand = EditConfigEntryC.ORIGIN;
    RemoveOriginCommand = DeleteConfigStationC.ORIGIN;
    AddSegmentCommand = AddConfigSegmentC.INSTANCE;
    EditSegmentCommand = EditConfigEntryC.SEGMENT;
    RemoveSegmentCommand = DeleteConfigSegmentC.INSTANCE;
    AddDestinationCommand = AddConfigStationC.DESTINATION;
    EditDestinationCommand = EditConfigEntryC.DESTINATION;
    RemoveDestinationCommand = DeleteConfigStationC.DESTINATION;

    Ebula.ServiceEditMode = false;
    Ebula.PropertyChanged += Ebula_PropertyChanged;

    LoadConfig();
  }

  public override void Destroy() {
    Ebula.PropertyChanged -= Ebula_PropertyChanged;
  }

  private void Ebula_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
    if (sender != Ebula) return;
    switch (e.PropertyName) {
      case nameof(Ebula.EditMode):
        OnPropertyChanged(nameof(CanSaveRoute));
        OnPropertyChanged(nameof(SelectedRoute));
        OnPropertyChanged(nameof(CanAddService));
        break;
      case nameof(Ebula.Hotkeys):
        OnPropertyChanged(nameof(HotkeyLabel));
        break;
    }
  }

  public void LoadConfig() {
    if (Ebula.Model.Config is not EbulaConfig config) return;
    OnPropertyChanged(nameof(LoadLabel));
    Logger?.LogDebug("Loading UI values from EBuLa Config {Config}", Ebula.Model.Config);
    ConfigName = config.Name;

    Routes.Clear();
    Routes.AddRange(config.Routes.Values.Select(r => new EbulaRouteVM(this, r)));
    RouteOverview.Clear();

    CustomRoute.Clear();
    CustomRoute.Add(new EbulaCustomEntryVM(this, config));
  }


  #region Properties
  public BaseC EditRouteCommand { get; }
  public string LoadLabel => Ebula.Model.LoadedConfigs.Count == 1 ? Ebula.Model.Config.IsEmpty ? "LOAD" : "NEW" : "NEW";
  public BaseC AddOriginCommand { get; }
  public BaseC EditOriginCommand { get; }
  public BaseC RemoveOriginCommand { get; }
  public BaseC EditServiceCommand { get; }
  public BaseC AddSegmentCommand { get; }
  public BaseC EditSegmentCommand { get; }
  public BaseC RemoveSegmentCommand { get; }
  public BaseC AddDestinationCommand { get; }
  public BaseC EditDestinationCommand { get; }
  public BaseC RemoveDestinationCommand { get; }
  public bool CanSaveRoute => Ebula.EditMode && UsingCustom;
  public BaseC SaveRouteCommand { get; }
  public string ConfigName {
    get {
      if (Ebula.Model.Config is EbulaConfig cfg)
        return cfg.Name;
      if (Ebula.Model.LoadedConfigs.Count > 1)
        return $"Config collection ({Ebula.Model.LoadedConfigs.Count})";
      return string.Empty;
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
      OnPropertyChanged(nameof(UsingServices));
      OnPropertyChanged(nameof(CanAddService));
      OnPropertyChanged(nameof(UsingOverview));
      OnPropertyChanged(nameof(CanSaveRoute));
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
      OnPropertyChanged(nameof(UsingServices));
      OnPropertyChanged(nameof(CanAddService));
      OnPropertyChanged(nameof(UsingOverview));
      OnPropertyChanged(nameof(CanSaveRoute));
    }
  }

  public bool CanAddService => UsingServices && Ebula.EditMode && _selectedRoute is not null;
  private bool _usingServices = false;
  public bool UsingServices {
    get => _usingServices && _usingRoutes;
    set {
      _usingServices = value;
      OnPropertyChanged(nameof(UsingServices));
      OnPropertyChanged(nameof(CanAddService));
      OnPropertyChanged(nameof(UsingOverview));
      OnPropertyChanged(nameof(ServiceLabel));
    }
  }

  public bool UsingOverview {
    get => !_usingServices && _usingRoutes;
    set {
      _usingServices = !value;
      OnPropertyChanged(nameof(UsingServices));
      OnPropertyChanged(nameof(CanAddService));
      OnPropertyChanged(nameof(UsingOverview));
      OnPropertyChanged(nameof(ServiceLabel));
    }
  }

  public string ServiceLabel => UsingServices ? "OVERVIEW" : "SERVICES";
  public ObservableCollection<EbulaRouteVM> Routes { get; } = new();

  private EbulaServiceVM? _selectedService;
  public EbulaServiceVM? SelectedService {
    get {
      return _selectedService;
    }
    set {
      _selectedService = value;
      ServiceStart = value?.StartTime ?? TimeSpan.Zero;
      ServiceName = value?.Name ?? "000000";
      OnPropertyChanged(nameof(SelectedService));
    }
  }

  private TimeSpan _serviceStart = TimeSpan.Zero;
  public TimeSpan ServiceStart {
    get {
      return _serviceStart;
    }
    set {
      _serviceStart = value;
      OnPropertyChanged(nameof(ServiceStart));
      OnPropertyChanged(nameof(FormattedServiceStart));
    }
  }

  public string FormattedServiceStart => ServiceStart.ToString("hh':'mm':'ss");

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

  private EbulaRouteVM? _selectedRoute;
  public EbulaRouteVM? SelectedRoute {
    get {
      return _selectedRoute;
    }
    set {
      _selectedRoute = value;
      RouteOverview.Clear();
      RouteServices.Clear();
      if (value is not null) {
        Logger?.LogInformation("Predefined Route {Route} selected", value);
        RouteOverview.AddRange(value.GenerateOverview());
        RouteServices.AddRange(value.ListServices());
      }
      OnPropertyChanged(nameof(SelectedRoute));
      OnPropertyChanged(nameof(CanAddService));
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

  public string HotkeyLabel => Ebula.Hotkeys ? "+ HK +" : "- HK -";

  public string ApiLabel => GameApi is not null ? "+ API +" : "- API -";
  public IEbulaGameApi? GameApi {
    get => Ebula.GameApi;
    set {
      Ebula.GameApi = value;
      OnPropertyChanged(nameof(ApiLabel));
    }
  }

  public ObservableCollection<EbulaRouteEntryVM> RouteOverview { get; } = new();

  public ObservableCollection<EbulaServiceVM> RouteServices { get; } = new();

  public ObservableCollection<EbulaCustomEntryVM> CustomRoute { get; } = new();


  #endregion
}
