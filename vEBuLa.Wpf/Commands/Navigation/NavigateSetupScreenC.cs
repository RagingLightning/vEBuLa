using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.Extensions;
using vEBuLa.Models;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands.Setup;
internal partial class NavigateSetupScreenC : NavigateScreenC {
  private ILogger<NavigateSetupScreenC>? Logger => App.GetService<ILogger<NavigateSetupScreenC>>();
  private SetupScreenVM Screen;

  public NavigateSetupScreenC(SetupScreenVM screen) {
    Screen = screen;
  }

  protected override void Accept() {// Start EBuLa with selection
    base.Accept();

    Logger?.LogInformation("Switching to main EBuLa screen");
    if (Screen.UsingRoutes) {
      if (Screen.SelectedRoute?.ReadOnly == true) {
        if (Screen.SelectedService is null) {
          Logger?.LogWarning("Routes from other configurations can't be edited");
          return;
        }
        Screen.Ebula.LoadService(Screen.SelectedService.Model);
      }
      else if (Screen.UsingServices) {
        if (Screen.SelectedService is null) {
          Logger?.LogWarning("No Service is selected. Remaining on Setup screen");
          return;
        }
        Screen.Ebula.LoadService(Screen.SelectedService.Model);
      }
      else {
        if (Screen.SelectedRoute is null) {
          Logger?.LogWarning("No Route is selected. Remaining on Setup screen");
          return;
        }
        Screen.Ebula.LoadSegments(Screen.SelectedRoute.Model.Segments, Screen.ServiceName, Screen.ServiceStart);
      }
    }
    else {
      if (Screen.CustomRoute.Where(e => e.SelectedSegment is not null).Count() == 0) {
        Logger?.LogWarning("Custom Route contains no Segments. Remaining on Setup screen.");
        return;
      }
      Screen.Ebula.LoadSegments(Screen.CustomRoute.Where(e => e.SelectedSegment is not null).Select(e => e.SelectedSegment!.Model), Screen.ServiceName, Screen.ServiceStart);
    }
  } // Start EBuLa with selection

  protected override void St() {
    base.St();

    Logger?.LogInformation("Switching to main EBuLa screen");
    Screen.Ebula.Screen = new EbulaScreenVM(Screen.Ebula);
    Screen.Destroy();
  }

  protected override void Button0() { // Load or create new EBuLa Config
    base.Button0();

    Logger?.LogDebug("Loading new EBuLa Config");
    if (Screen.Ebula.Model.Config is EbulaConfig cfg && cfg.IsEmpty) {
      var dialog = new OpenFileDialog {
        FileName = "vEBuLa",
        DefaultExt = ".ebula",
        Filter = "vEBuLa config files|*.ebula|Json-Files|*.json|All Files|*.*",
        InitialDirectory = App.ConfigFolder
      };

      if (dialog.ShowDialog() != true) return;
      Logger?.LogInformation("Loading EBuLa Config {ConfigFile}", dialog.FileName.BiCrop(10, 30));

      try {
        Screen.Ebula.Model = new Ebula(dialog.FileName, true);
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
      Screen.Ebula.Model = new Ebula(null, true);
      Screen.Status = "";
    }
    Screen.LoadConfig();
  } // Load or create new EBuLa Config

  protected override void Button1() { // Save current EBuLa Config
    base.Button1();

    Logger?.LogDebug("Saving EBuLa Config {Config}", Screen.Ebula.Model.Config);
    var dialog = new SaveFileDialog {
      FileName = "vEBuLa",
      DefaultExt = ".ebula",
      Filter = "vEBuLa config files|*.ebula|Json-Files|*.json|All Files|*.*",
      InitialDirectory = App.ConfigFolder
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

  protected override void Button2() { // Toggle between predefined and custom Route mode
    base.Button2();
    if (Screen.UsingRoutes) Logger?.LogInformation("Switching to custom route creation");
    else Logger?.LogInformation("Switching to predefined route");

    Screen.UsingRoutes = !Screen.UsingRoutes;
    Screen.UsingServices = false;
    Screen.Ebula.ServiceEditMode = false;
  } // Toggle between predefined and custom Route mode

  protected override void Button3() { // Toggle between route overview and service list
    base.Button3();
    Screen.UsingServices = !Screen.UsingServices;
    Screen.Ebula.ServiceEditMode = Screen.UsingServices;
  } // Toggle between route overview and service list

  protected override void Button4() { // Add new Service
    base.Button4();
    if (!Screen.CanAddService) return;
    if (Screen.Ebula.Model.Config is null) return;
    if (Screen.SelectedRoute is null) return;
    var oldService = Screen.SelectedService as EbulaServiceVM;

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditServiceDialog(oldService?.StartTime ?? TimeSpan.Zero, oldService?.Vehicles ?? string.Empty, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow)) - new Point(120, 220));

    if (dialog.ShowDialog() == false) {
      Logger?.LogDebug("{EditType} edit aborted by user", "Service");
      return;
    }

    if (EditServiceDialog.Delete) {
      return;
    }

    EbulaService service = Screen.Ebula.Model.Config.AddService(Screen.SelectedRoute.Model, EditServiceDialog.ServiceName, EditServiceDialog.StartTime, EditServiceDialog.Description, EditServiceDialog.Vehicle.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList());

    if (oldService is not null) {
      service.ShiftStops(oldService.Model.Stops, oldService.StartTime, EditServiceDialog.StartTime);
    }

    Screen.RouteServices.Add(service.ToVM(Screen.EditServiceCommand, Screen));
    Logger?.LogInformation("Created new service {Service}", service);
    Screen.Ebula.MarkDirty();
  } // Add new Service

  protected override void Button5() { // Save custom route as predefined route
    base.Button5();
    if (Screen.UsingRoutes) return;
    if (Screen.Ebula.Model.Config is null) return;
    if (!Screen.CustomRoute.Any(e => e.SelectedSegment is not null)) return;

    Logger?.LogInformation("Saving Custom Route {Route}", Screen.CustomRoute);
    var segments = Screen.CustomRoute.Where(e => e.SelectedSegment is not null).Select(e => e.SelectedSegment!.Model);

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditSavedRouteDialog(mainWindow.PointToScreen(Mouse.GetPosition(mainWindow)) - new Point(150, 20));

    if (dialog.ShowDialog() == false) {
      Logger?.LogDebug("{EditType} edit aborted by user", "Route Info");
      return;
    }

    EbulaRoute route = Screen.Ebula.Model.Config.AddRoute(segments, EditSavedRouteDialog.RouteName, EditSavedRouteDialog.Description, EditSavedRouteDialog.Stations, EditSavedRouteDialog.Duration, EditSavedRouteDialog.Route);
    Logger?.LogInformation("Saved Custom Route as {Route}", route);
    Screen.Ebula.MarkDirty();

    Screen.LoadConfig();
  } // Save custom route as predefined route

  protected override void Button6() {
    if (Screen.Ebula.Hotkeys) {
      Logger?.LogInformation("Disabling global hotkeys");
      Screen.Ebula.UnsetHotkeys();
    }
    else {
      Logger?.LogInformation("Enabling global hotkeys");
      Screen.Ebula.SetHotkeys();
    }
  }

  protected override void Button7() { // Toggle API functionality
    if (Screen.GameApi is IEbulaGameApi api) {
      Logger?.LogDebug("Disabling Game API");
      api.Dispose();
      Screen.GameApi = null;
      return;
    }

    Logger?.LogDebug("Enabling Game API");
    var mainWindow = Application.Current.MainWindow;
    var dialog = new SimpleTextBoxPopup(string.Empty, mainWindow.PointToScreen(new Point(500, 610)), new PopupOptions { Dark = true, CanResize = false, Height = 30, Width = 100, FontSize = 20, FontWeight = FontWeights.Bold });

    if (dialog.ShowDialog() == false) {
      Logger?.LogDebug("Game API Activation aborted");
      return;
    }
    Screen.GameApi = new EbulaTswApi(dialog.Text);
    Logger?.LogDebug("Game API Enabled with API key {Key}", dialog.Text);

  } // Toggle API functionality

  protected override void Button8() { // Set Service Identifier
    base.Button8();

    Logger?.LogDebug("Changing Service Identifier");
    var mainWindow = Application.Current.MainWindow;
    var dialog = new SimpleTextBoxPopup(Screen.ServiceName, mainWindow.PointToScreen(new Point(700, 610)), new PopupOptions { Dark = true, CanResize = false, Height = 30, Width = 100, FontSize = 20, FontWeight = FontWeights.Bold });

    if (dialog.ShowDialog() == false) {
      Logger?.LogDebug("Service Identifier change aborted");
      return;
    }

    Screen.ServiceName = dialog.Text;
    Logger?.LogInformation("Service Identifier changed to {Service}", Screen.ServiceName);
  } // Set Service Identifier

  protected override void Button9() { // Set Service Start time
    base.Button9();

    Logger?.LogDebug("Changing Service Start time");
    var mainWindow = Application.Current.MainWindow;
    var dialog = new SimpleTextBoxPopup(Screen.ServiceStart.ToString(@"hh\:mm\:ss"), mainWindow.PointToScreen(new Point(770, 610)), new PopupOptions { Dark = true, CanResize = false, Height = 30, Width = 100, FontSize = 20, FontWeight = FontWeights.Bold });

    if (dialog.ShowDialog() == false) {
      Logger?.LogDebug("Service Start time change aborted");
      return;
    }

    if (ShortTime().IsMatch(dialog.Text) && TimeSpan.TryParse($"{dialog.Text[..2]}:{dialog.Text[2..4]}:{dialog.Text[4..]}", out var t)
      || Time().IsMatch(dialog.Text) && TimeSpan.TryParse(dialog.Text, out t)) {
      Screen.ServiceStart = t;
    }
    Logger?.LogInformation("Service Start time changed to {Departure}", Screen.ServiceStart.ToString(@"hh\:mm\:ss"));
  } // Set Service Start time

  [GeneratedRegex("\\d{2}:\\d{2}:\\d{2}")]
  private static partial Regex Time();

  [GeneratedRegex("\\d{6}")]
  private static partial Regex ShortTime();
}
