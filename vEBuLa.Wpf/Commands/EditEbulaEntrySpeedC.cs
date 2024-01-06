using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class EditEbulaEntrySpeedC : BaseC {
  private ILogger<EditEbulaEntrySpeedC>? Logger => App.GetService<ILogger<EditEbulaEntrySpeedC>>();
  public static readonly EditEbulaEntrySpeedC INSTANCE = new();
  public override void Execute(object? parameter) {
    if (parameter is not EbulaEntryVM entry) return;
    if (entry.Screen is null) return;

    Logger?.LogInformation("Startind {EditType} edit for EbulaEntry {EbulaEntry}", "Speed", entry);

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditEntrySpeedDialog(entry.SpeedLimit, entry.SpeedSigned, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow))-new Point(75,50));

    if (dialog.ShowDialog() == false) {
      Logger?.LogInformation("{EditType} edit aborted by user", "Speed");
      return;
    }
    entry.SpeedLimit = EditEntrySpeedDialog.Speed;
    entry.SpeedSigned = EditEntrySpeedDialog.Signed;

    Logger?.LogInformation("{EditType} edit for EbulaEntry {EbulaEntry} complete", "Speed", entry);

    entry.Screen.UpdateEntries();
  }
}
