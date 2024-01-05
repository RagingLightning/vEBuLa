using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Dialogs;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class EditEbulaEntrySpeedC : BaseC {
  public static readonly EditEbulaEntrySpeedC INSTANCE = new();
  public override void Execute(object? parameter) {
    if (parameter is not EbulaEntryVM entry) return;
    if (entry.Model is null) return;
    if (entry.Screen is null) return;

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditEntrySpeedDialog(entry.SpeedLimit, entry.SpeedSigned, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow))-new Point(75,50));

    if (dialog.ShowDialog() == false) return;
    entry.Model.SpeedLimit = EditEntrySpeedDialog.Speed;
    entry.Model.SpeedSigned = EditEntrySpeedDialog.Signed;

    entry.Screen.UpdateEntries();
  }
}
