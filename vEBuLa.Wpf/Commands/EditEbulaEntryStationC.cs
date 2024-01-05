using System.Windows;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class EditEbulaEntryStationC : BaseC {
  public static readonly EditEbulaEntryStationC INSTANCE = new();
  public override void Execute(object? parameter) {
    if (parameter is not EbulaEntryVM entry) return;
    if (entry.Model is null) return;
    if (entry.Screen is null) return;

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditEntryNameDialog(entry.MainLabel, entry.SecondaryLabel, entry.MainBold, entry.SecondaryBold, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow))-new Point(75,50));

    if (dialog.ShowDialog() == false) return;
    entry.Model.LocationName = EditEntryNameDialog.EntryName;
    entry.Model.LocationNotes = EditEntryNameDialog.Description;
    entry.Model.LocationNameBold = EditEntryNameDialog.NameBold;
    entry.Model.LocationNotesBold = EditEntryNameDialog.DescriptionBold;

    entry.Screen.UpdateEntries();
  }
}
