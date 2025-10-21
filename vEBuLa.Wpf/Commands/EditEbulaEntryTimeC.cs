using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class EditEbulaEntryTimeC : BaseC {
  private ILogger<EditEbulaEntryTimeC>? Logger => App.GetService<ILogger<EditEbulaEntryTimeC>>();
  public static readonly EditEbulaEntryTimeC ARRIVAL = new(false);
  public static readonly EditEbulaEntryTimeC DEPARTURE = new(true);
  private readonly bool IsDeparture;

  public EditEbulaEntryTimeC(bool isDeparture) { IsDeparture = isDeparture; }
  public override void Execute(object? parameter) {
    if (parameter is not EbulaEntryVM entry) return;
    if (entry.Screen is null) return;
    Logger?.LogInformation("Starting {EditType} edit for EbulaEntry {EbulaEntry}", IsDeparture ? "Departure Time" : "Arrival Time", entry);

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditDateTimeDialog(IsDeparture ? entry.Departure : entry.Arrival, IsDeparture ? DateTimeDialogType.DEPARTURE : DateTimeDialogType.ARRIVAL, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow))-new Point(75,50));

    if (dialog.ShowDialog() == false) {
      Logger?.LogDebug("{EditType} edit aborted by user", IsDeparture ? "Departure Time" : "Arrival Time");
      return;
    }

    if (IsDeparture) entry.Departure = EditDateTimeDialog.Date;
    else {
      if (entry.Arrival is DateTime a && entry.Departure is DateTime d && EditDateTimeDialog.Date is DateTime n)
        entry.Departure = d.Add(n.Subtract(a));
      entry.Arrival = EditDateTimeDialog.Date;
    }

    Logger?.LogInformation("{EditType} edit on EbulaEntry {EbulaEntry} complete", IsDeparture ? "Departure Time" : "Arrival Time", entry);

    entry.Screen.UpdateEntries();
    entry.Screen.Ebula.MarkDirty();
  }
}
