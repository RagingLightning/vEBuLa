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

namespace vEBuLa.Commands.Setup;
internal class AddConfigStationC : BaseC {
  private ILogger<AddConfigStationC>? Logger => App.GetService<ILogger<AddConfigStationC>>();
  private bool IsOrigin;

  public static readonly AddConfigStationC ORIGIN = new(true);
  public static readonly AddConfigStationC DESTINATION = new(false);

  private AddConfigStationC(bool isOrigin) {
    IsOrigin = isOrigin;
  }

  public override void Execute(object? parameter) {
    if (parameter is not EbulaCustomEntryVM entry) return;
    if (IsOrigin && entry.Origins is null) return;
    if (!IsOrigin && entry.Destinations is null) return;
    if (entry.Screen.Ebula.Model.Config is not EbulaConfig config) return;
    Logger?.LogInformation("Adding new {StationType} station for Custom Route Entry {RouteEntry}", IsOrigin ? "Origin" : "Destination", entry);

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditStationDialog("Add Station", "", mainWindow.PointToScreen(Mouse.GetPosition(mainWindow)) - new Point(75, 50));

    if (dialog.ShowDialog() == false) {
      Logger?.LogDebug("Action aborted by user");
      return;
    }

    EbulaStation station = config.AddStation(EditStationDialog.StationName);

    if (IsOrigin) { entry.SelectedOrigin = station.ToVM(); if (entry.SelectedSegment is not null) entry.SelectedSegment.Origin = (station.Id, station.ToVM()); }
    else { entry.SelectedDestination = station.ToVM(); if (entry.SelectedSegment is not null) entry.SelectedSegment.Destination = (station.Id, station.ToVM()); }
    Logger?.LogInformation("New {StationType} added for Custom Route Entry {RouteEntry}", IsOrigin ? "Origin" : "Destination", entry);

    TimeSpan departure = entry.Screen.Ebula.ServiceStartTime;
    for (var i = 0; i < entry.Screen.CustomRoute.Count; i++) {
      var result = entry.Screen.CustomRoute[i].Validate(departure);
      if (!result.Valid) {
        Logger?.LogDebug("Route Entry {RouteEntry} is no longer valid", entry.Screen.CustomRoute[i]);
        break;
      }
      departure += result.nextDeparture;
    }
    entry.Screen.Ebula.MarkDirty();
  }
}
