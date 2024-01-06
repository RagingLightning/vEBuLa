using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class EditSegmentDurationC : BaseC {
  private ILogger<EditSegmentDurationC>? Logger => App.GetService<ILogger<EditSegmentDurationC>>();
  public static readonly EditSegmentDurationC INSTANCE = new();

  public override void Execute(object? parameter) {
    if (parameter is not EbulaMarkerEntryVM entry) return;
    Logger?.LogInformation("Starting {EditType} edit for EbulaSegment {EbulaSegment}", "Duration", entry.Segment);

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditTimeSpanDialog(entry.Segment.Duration, TimeSpanDialogType.DURATION, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow)) - new Point(75, 50));

    if (dialog.ShowDialog() == false) {
      Logger?.LogDebug("{EditType} edit aborted by user", "Duration");
      return;
    }
    if (EditTimeSpanDialog.Time is null) return;

    entry.Duration = (TimeSpan) EditTimeSpanDialog.Time;


    Logger?.LogInformation("{EditType} edit on EbulaSegment {EbulaSegment} complete", "Duration", entry.Segment);
  }
}
