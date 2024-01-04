using System.Windows;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class EditEbulaEntryTimeC : BaseC {
  public static readonly EditEbulaEntryTimeC ARRIVAL = new(false);
  public static readonly EditEbulaEntryTimeC DEPARTURE = new(true);
  private readonly bool IsDeparture;

  public EditEbulaEntryTimeC(bool isDeparture) { IsDeparture = isDeparture; }
  public override void Execute(object? parameter) {
    if (parameter is not EbulaEntryVM entry) return;
    if (entry.Model is null) return;
    if (entry.Screen is null) return;

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditTimeSpanDialog(IsDeparture ? entry.Departure : entry.Arrival, IsDeparture, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow))-new Point(75,50));

    if (dialog.ShowDialog() == false) return;
    if (IsDeparture) entry.Model.Departure = EditTimeSpanDialog.Time;
    else entry.Model.Arrival = EditTimeSpanDialog.Time;

    entry.Screen.UpdateEntries();
  }
}
