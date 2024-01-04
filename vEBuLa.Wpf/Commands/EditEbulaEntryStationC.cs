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
    var dialog = new EditStationDialog(entry.MainLabel, entry.SecondaryLabel, entry.MainBold, entry.SecondaryBold, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow))-new Point(75,50));

    if (dialog.ShowDialog() == false) return;
    entry.Model.LocationName = EditStationDialog.Name;
    entry.Model.LocationNotes = EditStationDialog.Description;
    entry.Model.LocationNameBold = EditStationDialog.NameBold;
    entry.Model.LocationNotesBold = EditStationDialog.DescriptionBold;

    entry.Screen.UpdateEntries();
  }
}
