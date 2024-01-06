using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.Models;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class SaveCustomRouteC : BaseC {
  private ILogger<SaveCustomRouteC>? Logger = App.GetService<ILogger<SaveCustomRouteC>>();
  private StorageConfigScreenVM Screen { get; }

  public SaveCustomRouteC(StorageConfigScreenVM screen) {
    Screen = screen;
  }
  public override void Execute(object? parameter) {
    if (Screen.UsingRoutes) return;
    if (Screen.CustomRoute.Count < 2) return;
    if (Screen.Ebula.Model.Config is null) return;
    var segments = Screen.CustomRoute.Where(e => e.SelectedSegment is not null).Select(e => e.SelectedSegment.Model);
    Logger?.LogInformation("Starting {EditType} edit for new Route", "Route Info");

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditSavedRouteDialog(mainWindow.PointToScreen(Mouse.GetPosition(mainWindow)) - new Point(150, 20));

    if (dialog.ShowDialog() == false) {
      Logger?.LogDebug("{EditType} edit aborted by user", "Route Info");
      return;
    }

    EbulaRoute route = Screen.Ebula.Model.Config.AddRoute(segments, EditSavedRouteDialog.RouteName, EditSavedRouteDialog.Description, EditSavedRouteDialog.Stations, EditSavedRouteDialog.Duration, EditSavedRouteDialog.Route);
    Logger?.LogInformation("{EditType} edit on new Route {Route} complete", "Route Info", route);
  }
}
