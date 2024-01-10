using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using vEBuLa.Dialogs;
using vEBuLa.Models;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands.Setup;
internal class EditConfigEntryC : BaseC {
  private ILogger<EditConfigEntryC>? Logger => App.GetService<ILogger<EditConfigEntryC>>();
  private bool IsStation, IsOrigin;

  public static readonly EditConfigEntryC SEGMENT = new(false, false);
  public static readonly EditConfigEntryC ORIGIN = new(true, true);
  public static readonly EditConfigEntryC DESTINATION = new(true, false);

  private EditConfigEntryC(bool isStation, bool isOrigin) {
    IsStation = isStation;
    IsOrigin = isOrigin;
  }

  public override void Execute(object? parameter) {
    if (parameter is not EbulaCustomEntryVM entry) return;
    if (entry.Screen.Ebula.Model.Config is not EbulaConfig config) return;
    Logger?.LogInformation("Editing {EditType} name for Custom Route Entry {RouteEntry}", IsStation ? IsOrigin ? "Origin" : "Destination" : "Segment", entry);


    string? name;
    if (IsStation) {
      if (IsOrigin) name = entry.Origin?.Name ?? entry.SelectedOrigin?.Name;
      else name = entry.Destination?.Name ?? entry.SelectedDestination?.Name;
    }
    else name = entry.SelectedSegment?.Name;

    if (name is null) return;

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditStationDialog($"Edit {(IsStation ? IsOrigin ? "Origin" : "Destination" : "Segment")}", name, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow)) - new Point(75, 50));

    if (dialog.ShowDialog() == false) {
      Logger?.LogDebug("Action aborted by user");
      return;
    }

    if (IsStation) {
      if (IsOrigin) {
        if (entry.Origin is not null) entry.Origin.Name = EditStationDialog.StationName;
        else if (entry.SelectedOrigin is not null) entry.SelectedOrigin.Name = EditStationDialog.StationName;
      }
      else {
        if (entry.Destination is not null) entry.Destination.Name = EditStationDialog.StationName;
        else if (entry.SelectedDestination is not null) entry.SelectedDestination.Name = EditStationDialog.StationName;
      }
    }
    else if (entry.SelectedSegment is not null) entry.SelectedSegment.Name = EditStationDialog.StationName;

    Logger?.LogInformation("Name of {EditType} changed for Custom Route Entry {RouteEntry}", IsStation ? IsOrigin ? "Origin" : "Destination" : "Segment", entry);

    TimeSpan departure = entry.Screen.Departure;
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
