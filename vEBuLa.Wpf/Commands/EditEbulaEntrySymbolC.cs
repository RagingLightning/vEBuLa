using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class EditEbulaEntrySymbolC : BaseC {
  private ILogger<EditEbulaEntrySymbolC>? Logger => App.GetService<ILogger<EditEbulaEntrySymbolC>>();
  public static readonly EditEbulaEntrySymbolC INSTANCE = new();
  public override void Execute(object? parameter) {
    if (parameter is not EbulaEntryVM entry) return;
    if (entry.Model is null) return;
    if (entry.Screen is null) return;
    Logger?.LogInformation("Starting {EditType} edit for EbulaEntry {EbulaEntry}", "Symbol", entry.Model);

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditEntrySymbolDialog(entry.Model.Symbol, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow))-new Point(75,50));

    if (dialog.ShowDialog() == false) {

      Logger?.LogDebug("{EditType} edit aborted by user", "Symbol");
      return;
    }
    entry.Model.Symbol = EditEntrySymbolDialog.Symbol;


    Logger?.LogInformation("{EditType} edit on EbulaEntry {EbulaEntry} complete", "Symbol", entry.Model);

    entry.Screen.UpdateEntries();
  }
}
