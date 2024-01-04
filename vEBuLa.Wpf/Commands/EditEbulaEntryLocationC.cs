using System.Windows;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class EditEbulaEntryLocationC : BaseC {
  public static readonly EditEbulaEntryLocationC INSTANCE = new();
  public override void Execute(object? parameter) {
    if (parameter is not EbulaEntryVM entry) return;
    if (entry.Model is null) return;
    if (entry.Screen is null) return;

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditLocationDialog(entry.Location, entry.Gradient, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow))-new Point(75,50));

    if (dialog.ShowDialog() == false) return;
    entry.Model.Location = EditLocationDialog.Location;
    entry.Model.GradientMark = EditLocationDialog.Gradient;

    entry.Screen.UpdateEntries();
  }
}
