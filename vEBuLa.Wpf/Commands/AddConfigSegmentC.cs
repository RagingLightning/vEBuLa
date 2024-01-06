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
internal class AddConfigSegmentC : BaseC {
  private ILogger<AddConfigSegmentC>? Logger => App.GetService<ILogger<AddConfigSegmentC>>();

  public static readonly AddConfigSegmentC INSTANCE = new();

  public override void Execute(object? parameter) {
    if (parameter is not EbulaCustomEntryVM entry) return;
    if (entry.Origin is null && entry.SelectedOrigin is null) return;
    if (entry.Screen.Ebula.Model.Config is not EbulaConfig config) return;
    Logger?.LogInformation("Adding new Segment for Custom Route Entry {RouteEntry}", entry);

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditStationDialog("Add Segmant", "", mainWindow.PointToScreen(Mouse.GetPosition(mainWindow)) - new Point(75, 50));

    if (dialog.ShowDialog() == false) {
      Logger?.LogDebug("Action aborted by user");
      return;
    }

    EbulaSegment segment = config.AddSegment(EditStationDialog.StationName, entry.Origin?.Model ?? entry.SelectedOrigin?.Model);

    entry.SelectedSegment = segment.ToVM();
    Logger?.LogInformation("Segment {EbulaSegment} added for Custom Route Entry {RouteEntry}", segment, entry);

    TimeSpan departure = entry.Screen.Departure;
    for (var i = 0; i < entry.Screen.CustomRoute.Count; i++) {
      var result = entry.Screen.CustomRoute[i].Validate(departure);
      if (!result.Valid) {
        Logger?.LogDebug("Route Entry {RouteEntry} is no longer valid", entry.Screen.CustomRoute[i]);
        break;
      }
      departure += result.nextDeparture;
    }
  }
}
