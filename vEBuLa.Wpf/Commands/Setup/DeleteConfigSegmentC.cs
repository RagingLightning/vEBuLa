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
internal class DeleteConfigSegmentC : BaseC {
  private ILogger<DeleteConfigSegmentC>? Logger => App.GetService<ILogger<DeleteConfigSegmentC>>();

  public static readonly DeleteConfigSegmentC INSTANCE = new();

  public override void Execute(object? parameter) {
    if (parameter is not EbulaCustomEntryVM entry) return;
    if (entry.Segments is null || entry.SelectedSegment is null) return;
    if (entry.Screen.Ebula.Model.Config is not EbulaConfig config) return;
    Logger?.LogInformation("Removing Segment {EbulaSegment} from Custom Route Entry {RouteEntry}", entry.SelectedSegment, entry);

    config.Segments.Remove(entry.SelectedSegment.Id);
    Logger?.LogInformation("Segment {EbulaSegment} removed from Custom Route Entry {RouteEntry}", entry.SelectedSegment, entry);

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
