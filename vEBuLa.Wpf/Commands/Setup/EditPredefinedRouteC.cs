using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands.Setup;
internal class EditPredefinedRouteC : BaseC {
  private ILogger<EditPredefinedRouteC>? Logger = App.GetService<ILogger<EditPredefinedRouteC>>();
  private SetupScreenVM Screen { get; }

  public EditPredefinedRouteC(SetupScreenVM screen) {
    Screen = screen;
  }
  public override void Execute(object? parameter) {
    if (Screen.UsingCustom) return;
    if (parameter is not EbulaRouteVM route) return;
    Logger?.LogInformation("Starting {EditType} edit for predefined Route {Route}", "Route Info", route);

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditSavedRouteDialog(route.Name, route.Description, route.StationCount, route.Duration, route.RouteOverview, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow)) - new Point(150, 20));

    if (dialog.ShowDialog() == false) {
      Logger?.LogDebug("{EditType} edit aborted by user", "Route Info");
      return;
    }

    route.Name = EditSavedRouteDialog.RouteName;
    route.Description = EditSavedRouteDialog.Description;
    route.StationCount = EditSavedRouteDialog.Stations;
    route.Duration = EditSavedRouteDialog.Duration;
    route.RouteOverview = EditSavedRouteDialog.Route;

    Logger?.LogInformation("{EditType} edit on new Route {Route} complete", "Route Info", route);
    Screen.Ebula.MarkDirty();
  }
}
