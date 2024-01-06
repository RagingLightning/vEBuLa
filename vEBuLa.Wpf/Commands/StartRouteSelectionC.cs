using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class StartRouteSelectionC : BaseC {
  private ILogger<StartRouteSelectionC>? Logger => App.GetService<ILogger<StartRouteSelectionC>>();
  private StorageConfigScreenVM Screen { get; }

  public StartRouteSelectionC(StorageConfigScreenVM screen) {
    Screen = screen;
  }

  public override void Execute(object? parameter) {
    if (Screen.UsingRoutes && Screen.CurrentRoute is EbulaRouteVM route) {
      Screen.Ebula.Model.Segments.Clear();
      Screen.Ebula.Model.SetActiveSegments(route.Model.Segments, Screen.Departure);
    }
    else if (Screen.CustomRoute.Count > 1) {
      Screen.Ebula.Model.Segments.Clear();
      Screen.Ebula.Model.SetActiveSegments(Screen.CustomRoute.Where(e => e.SelectedSegment is not null).Select(e => e.SelectedSegment.Model), Screen.Departure);
    }
    else
      return;

    Screen.Ebula.Screen = new EbulaScreenVM(Screen.Ebula);
  }
}
