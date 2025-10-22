using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class EditEbulaEntryLocationC : BaseC {
  private ILogger<EditEbulaEntryLocationC>? Logger => App.GetService<ILogger<EditEbulaEntryLocationC>>();
  public static readonly EditEbulaEntryLocationC INSTANCE = new();
  public override void Execute(object? parameter) {
    if (parameter is not EbulaEntryVM entry) return;
    if (entry.Screen is null) return;
    Logger?.LogInformation("Starting {EditType} edit for EbulaEntry {EbulaEntry}", "Location", entry);

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditEntryLocationDialog(entry.Location, entry.GpsLocation, entry.KilometerBreak, entry.Gradient, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow))-new Point(75,50), entry.Screen.Ebula.GameApi);

    if (dialog.ShowDialog() == false) {
      Logger?.LogDebug("{EditType} edit aborted by user", "Location");
      return;
    }

    entry.Location = EditEntryLocationDialog.Location;
    entry.GpsLocation = EditEntryLocationDialog.GpsLocation;
    entry.Gradient = EditEntryLocationDialog.Gradient;
    entry.KilometerBreak = EditEntryLocationDialog.KilometerBreak;

    Logger?.LogInformation("{EditType} edit on EbulaEntry {EbulaEntry} complete", "Location", entry);

    entry.Screen.UpdateEntries();
    entry.Screen.Ebula.MarkDirty();
  }
}
