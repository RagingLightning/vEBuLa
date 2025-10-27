using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using vEBuLa.Commands;

namespace vEBuLa.ViewModels;

public class EbulaRouteVM : BaseVM {
  public override string ToString() => $"[{Model}]";
  public SetupScreenVM Screen { get; }
  public EbulaRoute Model { get; }

  public EbulaRouteVM(SetupScreenVM screen, EbulaRoute route) {
    Screen = screen;
    Model = route;
  }

  public BaseC EditCommand => Screen.EditRouteCommand;
  public string Name {
    get => Model.Name;
    set {
      Model.Name = value;
      OnPropertyChanged(nameof(Name));
    }
  }
  public string Description {
    get => Model.Description;
    set {
      Model.Description = value;
      OnPropertyChanged(nameof(Description));
    }
  }
  public string RouteOverview {
    get => Model.RouteOverview;
    set {
      Model.RouteOverview = value;
      OnPropertyChanged(nameof(RouteOverview));
    }
  }
  public int StationCount {
    get => Model.StationCount;
    set { 
      Model.StationCount = value; 
      OnPropertyChanged(nameof(StationCount));
    }
  }
  public TimeSpan Duration {
    get => Model.Duration;
    set { 
      Model.Duration = value;
      OnPropertyChanged(nameof(Duration));
      OnPropertyChanged(nameof(DurationText));
    }
  }
  public string DurationText => Duration.ToString("hh':'mm':'ss");

  internal IEnumerable<EbulaRouteEntryVM> GenerateOverview() {
    List<EbulaRouteEntryVM> result = new();
    var previousEntry = new EbulaRouteEntryVM(Screen, Model.Segments[0].ToVM(), null);
    result.Add(previousEntry);
    result.AddRange(Model.Segments.Select(e => previousEntry = new EbulaRouteEntryVM(Screen, e.ToVM(), previousEntry)));
    return result;
  }

  internal IEnumerable<EbulaServiceVM> ListServices() {
    if (Screen.Ebula.Model.Config is null)
      return [];

    return Screen.Ebula.Model.Config.Services.Values.Where(s => s.Route == Model).Select(e => e.ToVM(Screen.EditServiceCommand, Screen));
  }
}