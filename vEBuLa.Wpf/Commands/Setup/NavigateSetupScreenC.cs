using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.Extensions;
using vEBuLa.Models;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands.Setup;
internal class NavigateSetupScreenC : NavigateScreenC {
  private ILogger<NavigateSetupScreenC>? Logger => App.GetService<ILogger<NavigateSetupScreenC>>();
  private SetupScreenVM Screen;

  public NavigateSetupScreenC(SetupScreenVM screen) {
    Screen = screen;
  }

  protected override void Accept() {// Start EBuLa with selection
    base.Accept();

    Logger?.LogInformation("Switching to main EBuLa screen");
    if (Screen.UsingRoutes) {
      if (Screen.SelectedRoute is null) {
        Logger?.LogWarning("No Route is selected. Remaining on Setup screen");
        return;
      }
      Screen.Ebula.Model.SetActiveSegments(Screen.SelectedRoute.Model.Segments);
    }
    else {
      if (Screen.CustomRoute.Where(e => e.SelectedSegment is not null).Count() == 0) {
        Logger?.LogWarning("Custom Route contains no Segments. Remaining on Setup screen.");
        return;
      }
      Screen.Ebula.Model.SetActiveSegments(Screen.CustomRoute.Where(e => e.SelectedSegment is not null).Select(e => e.SelectedSegment.Model));
    }
    Logger?.LogDebug("EBuLa instance starts at {Departure} with Segments {Segments}", Screen.Ebula.ServiceStartTime, Screen.Ebula.Model.Segments);

    Screen.Ebula.Screen = new EbulaScreenVM(Screen.Ebula);
    Screen.Ebula.RunServiceClock();
    Screen.Destroy();
  } // Start EBuLa with selection

  protected override void Button0() { // Load or create new EBuLa Config
    base.Button0();

    Logger?.LogDebug("Loading new EBuLa Config");
    if (Screen.Ebula.Model.Config.IsEmpty) {
      var dialog = new OpenFileDialog {
        FileName = "vEBuLa",
        DefaultExt = ".json",
        Filter = "vEBuLa config files|*.json|All Files|*.*"
      };

      if (dialog.ShowDialog() != true) return;
      Logger?.LogInformation("Loading EBuLa Config {ConfigFile}", dialog.FileName.BiCrop(10, 30));

      try {
        Screen.Ebula.Model = new Ebula(dialog.FileName);
        Screen.Status = "";
      }
      catch (Exception ex) {
        Screen.Status = "FAILED TO LOAD CONFIG!";
        Logger?.LogError(ex, "Failed to load Config from {ConfigFile}", dialog.FileName.BiCrop(10, 30));
        return;
      }
    }
    else {
      Logger?.LogInformation("Loading new empty EBuLa Config");
      Screen.Ebula.Model = new Ebula(null);
      Screen.Status = "";
    }
    Screen.LoadConfig();
  } // Load or create new EBuLa Config

  protected override void Button1() { // Save current EBuLa Config
    base.Button1();

    Logger?.LogDebug("Saving EBuLa Config {Config}", Screen.Ebula.Model.Config);
    var dialog = new SaveFileDialog {
      FileName = "vEBuLa",
      DefaultExt = ".json",
      Filter = "vEBuLa config files|*.json|All Files|*.*"
    };

    if (dialog.ShowDialog() != true) return;

    try {
      Screen.Ebula.Model.Config?.Save(dialog.FileName);
      Logger?.LogInformation("EBuLa Config {Config} saved to {FileName}", Screen.Ebula.Model.Config, dialog.FileName.BiCrop(10, 30));
    }
    catch (Exception ex) {
      Logger?.LogError(ex, "Failed to save Config {Config} to {FileName}", Screen.Ebula.Model.Config, dialog.FileName.BiCrop(10, 30));
    }
  } // Save current EBuLa Config

  protected override void Button3() { // Toggle between predefined and custom Route mode
    base.Button3();
    if (Screen.UsingRoutes) Logger?.LogInformation("Switching to custom route creation");
    else Logger?.LogInformation("Switching to predefined route");

    Screen.UsingRoutes = !Screen.UsingRoutes;
  } // Toggle between predefined and custom Route mode

  protected override void Button5() { // Save custom route as predefined route
    base.Button5();

    if (Screen.UsingRoutes) return;
    if (!Screen.CustomRoute.Any(e => e.SelectedSegment is not null)) return;

    Logger?.LogInformation("Saving Custom Route {Route}", Screen.CustomRoute);
    var segments = Screen.CustomRoute.Where(e => e.SelectedSegment is not null).Select(e => e.SelectedSegment.Model);

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditSavedRouteDialog(mainWindow.PointToScreen(Mouse.GetPosition(mainWindow)) - new Point(150, 20));

    if (dialog.ShowDialog() == false) {
      Logger?.LogDebug("{EditType} edit aborted by user", "Route Info");
      return;
    }

    EbulaRoute route = Screen.Ebula.Model.Config.AddRoute(segments, EditSavedRouteDialog.RouteName, EditSavedRouteDialog.Description, EditSavedRouteDialog.Stations, EditSavedRouteDialog.Duration, EditSavedRouteDialog.Route);
    Logger?.LogInformation("Saved Custom Route as {Route}", route);
    Screen.Ebula.MarkDirty();
  } // Save custom route as predefined route

  protected override void Button8() { // Set Service Identifier
    base.Button8();

    Logger?.LogDebug("Changing Service Identifier");
    var mainWindow = Application.Current.MainWindow;
    var dialog = new SimpleTextBoxPopup(Screen.Ebula.ServiceName, mainWindow.PointToScreen(new Point(700, 610)), new PopupOptions { Dark = true, CanResize = false, Height = 30, Width = 100, FontSize = 20, FontWeight = FontWeights.Bold });

    if (dialog.ShowDialog() == false) {
      Logger?.LogDebug("Service Identifier change aborted");
      return;
    }

    Screen.Ebula.ServiceName = dialog.Text;
    Logger?.LogInformation("Service Identifier changed to {Service}", Screen.Ebula.ServiceName);
  } // Set Service Identifier

  protected override void Button9() { // Set Service Start time
    base.Button9();

    Logger?.LogDebug("Changing Service Start time");
    var mainWindow = Application.Current.MainWindow;
    var dialog = new SimpleTextBoxPopup(Screen.Ebula.ServiceStartTime.ToString(@"hh\:mm\:ss"), mainWindow.PointToScreen(new Point(770, 610)), new PopupOptions { Dark = true, CanResize = false, Height = 30, Width = 100, FontSize = 20, FontWeight = FontWeights.Bold });

    if (dialog.ShowDialog() == false) {
      Logger?.LogDebug("Service Start time change aborted");
      return;
    }

    Screen.Ebula.FormattedServiceStartTime = dialog.Text;
    Logger?.LogInformation("Service Start time changed to {Departure}", Screen.Ebula.ServiceStartTime.ToString(@"hh\:mm\:ss"));
  } // Set Service Start time
}
