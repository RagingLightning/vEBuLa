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
    var dialog = new EditSpeedDialog(entry.SpeedLimit, entry.SpeedSigned, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow))-new Point(75,50));

    if (dialog.ShowDialog() == false) return;
    entry.Model.SpeedLimit = EditSpeedDialog.Speed;
    entry.Model.SpeedSigned = EditSpeedDialog.Signed;

    entry.Screen.UpdateEntries();
  }
}
