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
internal class DeleteConfigStationC : BaseC {
  private ILogger<DeleteConfigStationC>? Logger => App.GetService<ILogger<DeleteConfigStationC>>();
  private bool IsOrigin;

  public static readonly DeleteConfigStationC ORIGIN = new(true);
  public static readonly DeleteConfigStationC DESTINATION = new(false);

  private DeleteConfigStationC(bool isOrigin) {
    IsOrigin = isOrigin;
  }

  public override void Execute(object? parameter) {
    if (parameter is not EbulaCustomEntryVM entry) return;
    if (IsOrigin && (entry.Origins is null || entry.SelectedOrigin is null)) return;
    if (!IsOrigin && (entry.Destinations is null || entry.SelectedDestination is null)) return;
    if (entry.Screen.Ebula.Model.Config is not EbulaConfig config) return;
    Logger?.LogInformation("Removing {StationType} station from Custom Route Entry {RouteEntry}", IsOrigin ? "Origin" : "Destination", entry);

    if (IsOrigin) config.Stations.Remove(entry.SelectedOrigin!.Id);
    else config.Stations.Remove(entry.SelectedDestination!.Id);
    Logger?.LogInformation("Removed {StationType} station from Custom Route Entry {RouteEntry}", IsOrigin ? "Origin" : "Destination", entry);

    TimeSpan departure = entry.Screen.ServiceStart;
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
