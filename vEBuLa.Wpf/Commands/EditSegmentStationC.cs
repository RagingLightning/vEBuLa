using System.Windows;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class EditSegmentStationC : BaseC {
  public static readonly EditSegmentStationC ORIGIN = new(false);
  public static readonly EditSegmentStationC DESTINATION = new(true);
  private readonly bool IsDestination;
  private EditSegmentStationC(bool isDestination) { IsDestination = isDestination; }
  public override void Execute(object? parameter) {
    if (parameter is not EbulaMarkerEntryVM entry) return;
    if (IsDestination && entry.Segment.Destination.Station is null
      || !IsDestination && entry.Segment.Origin.Station is null) return;

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditStationDialog(IsDestination ? entry.Segment.Destination.Station?.Name ?? "null" : entry.Segment.Origin.Station?.Name ?? "null", mainWindow.PointToScreen(Mouse.GetPosition(mainWindow)) - new Point(75, 50));

    if (dialog.ShowDialog() == false) return;
    if (IsDestination)
      entry.Segment.Destination.Station.Name = EditStationDialog.StationName;
    else
      entry.Segment.Origin.Station.Name = EditStationDialog.StationName;

    entry.Screen.UpdateEntries();
  }
}
