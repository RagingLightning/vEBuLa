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
internal class SetupScreenVM : BaseVM {
  private ILogger<SetupScreenVM>? Logger => App.GetService<ILogger<SetupScreenVM>>();
  public EbulaVM Ebula { get; }

  public SetupScreenVM(EbulaVM ebula){
    Ebula = ebula;
    Ebula.NavigateCommand = new NavigateSetupScreenC(this);

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

    Ebula.PropertyChanged += Ebula_PropertyChanged;
    Ebula.ConfigDirtyChanged += Ebula_ConfigDirtyChanged;

    LoadConfig();
  }

  public override void Destroy() {
    Ebula.PropertyChanged -= Ebula_PropertyChanged;
    Ebula.ConfigDirtyChanged -= Ebula_ConfigDirtyChanged;
  }

  private void Ebula_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
    if (sender != Ebula) return;
    switch (e.PropertyName) {
      case nameof(Ebula.EditMode):
        OnPropertyChanged(nameof(CanSaveRoute));
        OnPropertyChanged(nameof(SelectedRoute));
        break;
    }
  }

  private void Ebula_ConfigDirtyChanged() {
    OnPropertyChanged(nameof(CanSave));
  }

  public void LoadConfig() {
    if (Ebula.Model.Config is not EbulaConfig config) return;
    OnPropertyChanged(nameof(LoadLabel));
    OnPropertyChanged(nameof(CanSave));
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
  public string LoadLabel => Ebula.Model.Config.IsEmpty ? "LOAD" : "NEW";
  public bool CanSave => Ebula.Model.Config.IsDirty;
  public BaseC AddOriginCommand { get; }
  public BaseC EditOriginCommand { get; }
  public BaseC RemoveOriginCommand { get; }
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
      OnPropertyChanged(nameof(CanSaveRoute));
    }
  }

  public ObservableCollection<EbulaRouteVM> Routes { get; } = new();

  private EbulaRouteVM? _selectedRoute;
  public EbulaRouteVM? SelectedRoute {
    get {
      return Ebula.EditMode ? null : _selectedRoute;
    }
    set {
      _selectedRoute = value;
      RouteOverview.Clear();
      if (value is not null) {
        Logger?.LogInformation("Predefined Route {Route} selected", value);
        RouteOverview.AddRange(value.GenerateOverview());
      }
      if (Ebula.EditMode)
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


  #endregion
}
