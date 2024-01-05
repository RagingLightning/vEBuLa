using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class EditEbulaEntryTunnelC : BaseC {
  private ILogger<EditEbulaEntryTunnelC>? Logger => App.GetService<ILogger<EditEbulaEntryTunnelC>>();
  public static readonly EditEbulaEntryTunnelC INSTANCE = new();
  public override void Execute(object? parameter) {
    if (parameter is not EbulaEntryVM entry) return;
    if (entry.Model is null) return;
    if (entry.Screen is null) return;
    Logger?.LogInformation("Starting {EditType} edit for EbulaEntry {EbulaEntry}", "Tunnel", entry.Model);

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditEntryTunnelDialog(entry.TunnelStart, entry.TunnelEnd, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow))-new Point(75,50));

    if (dialog.ShowDialog() == false) {

      Logger?.LogDebug("{EditType} edit aborted by user", "Tunnel");
      return;
    }
    entry.Model.TunnelStart = EditEntryTunnelDialog.TunnelStart;
    entry.Model.TunnelEnd = EditEntryTunnelDialog.TunnelEnd;

    Logger?.LogInformation("{EditType} edit on EbulaEntry {EbulaEntry} complete", "Tunnel", entry.Model);

    entry.Screen.UpdateEntries();
  }
}
