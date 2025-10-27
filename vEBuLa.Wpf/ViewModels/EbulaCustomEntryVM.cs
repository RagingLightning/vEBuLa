using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using vEBuLa.Models;
using Windows.UI.Composition.Scenes;

namespace vEBuLa.ViewModels;

public class EbulaCustomEntryVM : BaseVM {
  private ILogger<EbulaCustomEntryVM>? Logger = App.GetService<ILogger<EbulaCustomEntryVM>>();
  public SetupScreenVM Screen { get; }
  private bool _modeChange = false;

  public EbulaCustomEntryVM(SetupScreenVM screen) {
    Screen = screen;
    Origins = new List<EbulaStationVM>();
    foreach (var config in Screen.Ebula.Model.LoadedConfigs) {
      Origins.AddRange(config.Value.Stations.Select(s => s.Value.ToVM()));
    }
    Departure = TimeSpan.Zero;

    Screen.Ebula.PropertyChanged += Ebula_PropertyChanged;
  }

  public EbulaCustomEntryVM(SetupScreenVM screen, EbulaConfig config, EbulaStationVM origin, TimeSpan departure) {
    Screen = screen;
    Origin = origin;
    if (Screen.Ebula.EditMode)
      Segments = config.Segments.Values.Select(e => e.ToVM()).ToList();
    else
      Segments = config.FindSegments(origin.Model).Select(e => e.ToVM()).ToList();
    Departure = departure;

    Screen.Ebula.PropertyChanged += Ebula_PropertyChanged;
  }

  public override void Destroy() {
    Screen.Ebula.PropertyChanged -= Ebula_PropertyChanged;
    base.Destroy();
  }

  public (bool Valid, TimeSpan nextDeparture) Validate(TimeSpan departure) {
    bool valid = true;
    valid &= Origin is null || Origins.Contains(Origin);
    valid &= SelectedOrigin is null || Origins.Contains(SelectedOrigin);

    if (!valid) {
      Logger?.LogError("Origin of {CustomEntry} is invalid, should have been caught before", this);
      Screen.LoadConfig();
      return (false, TimeSpan.Zero);
    }

    OnPropertyChanged(nameof(OriginText));
    valid &= SelectedSegment is null || Config.Segments.ContainsKey(SelectedSegment.Id);
    valid &= SelectedSegment is null || Origin is null || SelectedSegment.Origin.Station == Origin;
    valid &= SelectedSegment is null || SelectedOrigin is null || SelectedSegment.Origin.Station == SelectedOrigin;

    if (!valid) {
      SelectedSegment = null;
    }

    valid &= Destination is null || Config.Stations.ContainsKey(Destination.Id);
    valid &= SelectedSegment is null || Destination is null || SelectedSegment.Destination.Station == Destination;
    valid &= SelectedDestination is null || Config.Stations.ContainsKey(SelectedDestination.Id);
    valid &= SelectedSegment is null || SelectedDestination is null || SelectedSegment.Destination.Station == SelectedDestination;

    if (!valid) {
      SelectedDestination = null;
      Destination = null;

      ReduceRoute();
    }
    OnPropertyChanged(nameof(DestinationText));

    Origins = Origins is null ? null : Config.Stations.Values.Select(e => e.ToVM()).ToList();
    OnPropertyChanged(nameof(SelectedOrigin));
    if (Screen.Ebula.EditMode) {
      Segments = Config.Segments.Values.Select(e => e.ToVM()).ToList();
    }
    else {
      if (Origin is not null) Segments = Config.FindSegments(Origin.Model).Select(e => e.ToVM()).ToList();
    }
    OnPropertyChanged(nameof(SelectedSegment));
    Destinations = Destinations is null ? null : Config.Stations.Values.Select(e => e.ToVM()).ToList();
    OnPropertyChanged(nameof(SelectedDestination));

    return (valid, departure + SelectedSegment?.Duration ?? TimeSpan.Zero);
  }

  private void Ebula_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
    if (e.PropertyName == nameof(Screen.Ebula.EditMode)) {
      _modeChange = true;
      if (Screen.Ebula.EditMode) {
        Destinations = SelectedSegment is null ? null : Config.Stations.Values.Select(e => e.ToVM()).ToList();
        SelectedDestination = Destination;
        Destination = null;
      }
      else {
        Destination = SelectedDestination;
        Destinations = null;
      }
      _modeChange = false;
    }
  }

  private void ReduceRoute() {
    var index = Screen.CustomRoute.IndexOf(this);
    while (Screen.CustomRoute.Count > index + 1)
      Screen.CustomRoute.RemoveAt(index + 1);
  }

  private List<EbulaStationVM> _origins = [];
  public List<EbulaStationVM> Origins {
    get {
      return _origins;
    }
    set {
      _origins = value;
      OnPropertyChanged(nameof(Origins));
    }
  }

  private EbulaStationVM? _selectedOrigin;
  public EbulaStationVM? SelectedOrigin {
    get {
      return _selectedOrigin;
    }
    set {
      _selectedOrigin = value;
      if (!_modeChange) ReduceRoute();
      if (value is not null)
        Segments = Config.FindSegments(value.Model).Select(e => e.ToVM()).ToList();
      else
        Segments = null;
      OnPropertyChanged(nameof(SelectedOrigin));
    }
  }

  private EbulaStationVM? _origin;
  public EbulaStationVM? Origin {
    get {
      return _origin;
    }
    set {
      _origin = value;
      if (value is not null)
        Segments = Config.FindSegments(value.Model).Select(e => e.ToVM()).ToList();
      else
        Segments = null;
      OnPropertyChanged(nameof(Origin));
      OnPropertyChanged(nameof(OriginText));
    }
  }
  public string OriginText => Origin?.Name ?? string.Empty;

  private TimeSpan _departure;
  public TimeSpan Departure {
    get {
      return _departure;
    }
    set {
      _departure = value;
      OnPropertyChanged(nameof(Departure));
      OnPropertyChanged(nameof(DepartureText));
    }
  }

  public string DepartureText => (Departure + Screen.ServiceStart).ToString("hh':'mm':'ss");

  private List<EbulaSegmentVM>? _segments;
  public List<EbulaSegmentVM>? Segments {
    get {
      return _segments;
    }
    set {
      _segments = value;
      OnPropertyChanged(nameof(Segments));
    }
  }

  private EbulaSegmentVM? _selectedSegment;
  public EbulaSegmentVM? SelectedSegment {
    get {
      return _selectedSegment;
    }
    set {
      _selectedSegment = value;
      if (!_modeChange) ReduceRoute();
      if (Screen.Ebula.EditMode) {
        Destinations = Config.Stations.Values.Select(e => e.ToVM()).ToList();
        SelectedDestination = value?.Destination.Station;
      }
      else {
        Destination = value?.Destination.Station;
        if (value?.Destination.Station is not null)
          Screen.CustomRoute.Add(new EbulaCustomEntryVM(Screen, Config, value.Destination.Station, Departure + value.Duration));
      }
      OnPropertyChanged(nameof(SelectedSegment));
    }
  }

  private List<EbulaStationVM>? _destinations;
  public List<EbulaStationVM>? Destinations {
    get {
      return _destinations;
    }
    set {
      _destinations = value;
      OnPropertyChanged(nameof(Destinations));
    }
  }

  private EbulaStationVM? _selectedDestination;
  public EbulaStationVM? SelectedDestination {
    get {
      return _selectedDestination;
    }
    set {
      _selectedDestination = value;
      if (!_modeChange) ReduceRoute();
      if (Screen.Ebula.EditMode && value is not null && SelectedSegment is not null) {
        SelectedSegment.Destination = (value.Id, value);
        if (!_modeChange) Screen.CustomRoute.Add(new EbulaCustomEntryVM(Screen, Config, value, Departure + SelectedSegment.Duration));
      }
      OnPropertyChanged(nameof(SelectedDestination));
    }
  }

  private EbulaStationVM? _destination;
  public EbulaStationVM? Destination {
    get {
      return _destination;
    }
    set {
      _destination = value;
      OnPropertyChanged(nameof(Destination));
      OnPropertyChanged(nameof(DestinationText));
    }
  }
  public string DestinationText => Destination?.Name ?? string.Empty;
}