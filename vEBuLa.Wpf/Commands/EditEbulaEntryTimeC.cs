using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.Extensions;
using vEBuLa.Models;
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

    if (entry.Screen.Ebula.Service is null) {
      Logger?.LogWarning("Cannot edit EbulaEntry {EbulaEntry} in Service Edit Mode when no service is loaded", entry);
      return;
    }
    var stop = entry.Screen.Ebula.Service.Stops.Find(s => s.EntryLocation == entry.Location && s.EntryName == entry.MainLabel);
    if (stop is null) {
      var newStop = new EbulaServiceStop(entry.Model);
      entry.Screen.Ebula.Service.Stops.Add(newStop);
      entry.Stop = newStop;
    }

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditDateTimeDialog(IsDeparture ? entry.Departure : entry.Arrival, () => AutoTime(entry), IsDeparture ? DateTimeDialogType.DEPARTURE : DateTimeDialogType.ARRIVAL, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow)) - new Point(75, 50));

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

  private DateTime? AutoTime(EbulaEntryVM entry) {
    if (entry.GpsLocation is not Vector2 myPosition)
      return null;

    var prevStopEntry = entry.Screen.Entries.TakeWhile(e => e != entry).LastOrDefault(e =>
      e is EbulaEntryVM vm
      && vm.Stop is not null
      && vm.Departure is not null
      && vm.GpsLocation is not null) as EbulaEntryVM;
    var nextStopEntry = entry.Screen.Entries.SkipWhile(e => e != entry).Skip(1).FirstOrDefault(e =>
      e is EbulaEntryVM vm
      && vm.Stop is not null
      && (vm.Departure is not null || vm.Arrival is not null)
      && vm.GpsLocation is not null) as EbulaEntryVM;

    if (prevStopEntry is null || nextStopEntry is null)
      return null;

    var innerEntries = entry.Screen.Entries.SkipWhile(e => e != prevStopEntry).TakeWhile(e => e != nextStopEntry);

    if (prevStopEntry?.Departure is not DateTime startTime)
      return null;

    if ((nextStopEntry?.Arrival ?? nextStopEntry?.Departure) is not DateTime endTime)
      return null;

    if (!innerEntries.Any(e => e is EbulaEntryVM vm && vm.KilometerBreak)) { // Use location on track if no break for better accuracy
      Logger?.LogInformation("Extrapolating time at {Position} from {TimeA} at {PositionA} and {TimeB} at {PositionB}",
        entry.Location, startTime, prevStopEntry.Location, endTime, nextStopEntry.Location);
      return MathEx.ExtrapolateFromTimeFrame(prevStopEntry.Location, startTime, nextStopEntry.Location, endTime, entry.Location);
    }

    if (prevStopEntry?.GpsLocation is not Vector2 startPos)
      return null;

    if (nextStopEntry?.GpsLocation is not Vector2 endPos)
      return null;

    Logger?.LogInformation("Extrapolating time at {Position} from {TimeA} at {PositionA} and {TimeB} at {PositionB}",
      entry.Location, startTime, prevStopEntry.GpsLocation, endTime, nextStopEntry.GpsLocation);
    return MathEx.ExtrapolateFromTimeFrame(startPos, startTime, endPos, endTime, myPosition);
  }

}
