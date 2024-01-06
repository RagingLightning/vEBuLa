using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace vEBuLa.ViewModels;

internal class EbulaRouteVM {
  private StorageConfigScreenVM Screen { get; }
  public EbulaRoute Model { get; }

  public EbulaRouteVM(StorageConfigScreenVM screen, EbulaRoute route) {
    Screen = screen;
    Model = route;
  }

  public string Name => Model.Name;
  public string Description => Model.Description;
  public string RouteOverview => Model.RouteOverview;
  public int StationCount => Model.StationCount;
  public string Duration => Model.Duration.ToString("hh':'mm':'ss");

  internal IEnumerable<EbulaRouteEntryVM> GenerateOverview() {
    List<EbulaRouteEntryVM> result = new();
    var previousEntry = new EbulaRouteEntryVM(Screen, Model.Segments[0].ToVM(), null);
    result.Add(previousEntry);
    return Model.Segments.Select(e => previousEntry = new EbulaRouteEntryVM(Screen, e.ToVM(), previousEntry));
  }
}