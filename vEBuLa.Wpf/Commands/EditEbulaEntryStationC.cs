using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class EditEbulaEntryStationC : BaseC {
  private ILogger<EditEbulaEntryStationC>? Logger => App.GetService<ILogger<EditEbulaEntryStationC>>();
  public static readonly EditEbulaEntryStationC INSTANCE = new();
  public override void Execute(object? parameter) {
    if (parameter is not EbulaEntryVM entry) return;
    if (entry.Screen is null) return;

    Logger?.LogInformation("Starting {EditType} edit for EbulaEntry {EbulaEntry}", "Station", entry);

    if (entry.Screen.Ebula.ServiceEditMode) {
      entry.MainBold = !entry.MainBold;
    }
    else {

      var mainWindow = Application.Current.MainWindow;
      var dialog = new EditEntryNameDialog(entry.MainLabel, entry.SecondaryLabel, entry.MainBold, entry.SecondaryBold, entry.LabelBox, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow)) - new Point(75, 50));

      if (dialog.ShowDialog() == false) {
        Logger?.LogDebug("{EditType} edit aborted by user", "Station");
        return;
      }

      entry.MainLabel = EditEntryNameDialog.EntryName;
      entry.SecondaryLabel = EditEntryNameDialog.Description;
      entry.MainBold = EditEntryNameDialog.NameBold;
      entry.SecondaryBold = EditEntryNameDialog.DescriptionBold;
      entry.LabelBox = EditEntryNameDialog.LabelBox;

    }

    Logger?.LogInformation("{EditType} edit for EbulaEntry {EbulaEntry} complete", "Station", entry);

    entry.Screen.UpdateEntries();
    entry.Screen.Ebula.MarkDirty();
  }
}
