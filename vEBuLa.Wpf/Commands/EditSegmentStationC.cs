using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class EditSegmentStationC : BaseC {
  private ILogger<EditSegmentStationC>? Logger => App.GetService<ILogger<EditSegmentStationC>>();
  public static readonly EditSegmentStationC ORIGIN = new(false);
  public static readonly EditSegmentStationC DESTINATION = new(true);
  private readonly bool IsDestination;
  private EditSegmentStationC(bool isDestination) { IsDestination = isDestination; }
  public override void Execute(object? parameter) {
    if (parameter is not EbulaMarkerEntryVM entry) return;
    if (IsDestination && entry.Segment.Destination.Station is null
      || !IsDestination && entry.Segment.Origin.Station is null) return;
    Logger?.LogInformation("Starting {EditType} edit for EbulaSegment {EbulaSegment}", IsDestination ? "Destination Name" : "Origin Name", entry.Segment);

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditStationDialog("Change Station", IsDestination ? entry.Segment.Destination.Station?.Name ?? "null" : entry.Segment.Origin.Station?.Name ?? "null", mainWindow.PointToScreen(Mouse.GetPosition(mainWindow)) - new Point(75, 50));

    if (dialog.ShowDialog() == false) {
      Logger?.LogDebug("{EditType} edit aborted by user", IsDestination ? "Destination Name" : "Origin Name");
      return;
    }
    if (IsDestination)
      entry.Segment.Destination.Station.Name = EditStationDialog.StationName;
    else
      entry.Segment.Origin.Station.Name = EditStationDialog.StationName;

    Logger?.LogInformation("{EditType} edit on EbulaSegment {EbulaSegment} complete", IsDestination ? "Destination Name" : "Origin Name", entry.Segment);

    entry.Screen.UpdateEntries();
    entry.Screen.Ebula.MarkDirty();
  }
}
