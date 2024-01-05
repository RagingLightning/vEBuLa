using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class EditSegmentDurationC : BaseC {
  public static readonly EditSegmentDurationC INSTANCE = new();

  public override void Execute(object? parameter) {
    if (parameter is not EbulaMarkerEntryVM entry) return;

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditTimeSpanDialog(entry.Segment.Duration, TimeSpanDialogType.DURATION, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow)) - new Point(75, 50));

    if (dialog.ShowDialog() == false || EditTimeSpanDialog.Time is null) return;
    entry.Segment.Duration = (TimeSpan) EditTimeSpanDialog.Time;
    entry.Duration = entry.Segment.Duration.ToString("hh':'mm':'ss");
  }
}
