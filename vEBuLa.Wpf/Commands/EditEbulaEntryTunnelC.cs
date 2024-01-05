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
internal class EditEbulaEntryTunnelC : BaseC {
  public static readonly EditEbulaEntryTunnelC INSTANCE = new();
  public override void Execute(object? parameter) {
    if (parameter is not EbulaEntryVM entry) return;
    if (entry.Model is null) return;
    if (entry.Screen is null) return;

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditEntryTunnelDialog(entry.TunnelStart, entry.TunnelEnd, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow))-new Point(75,50));

    if (dialog.ShowDialog() == false) return;
    entry.Model.TunnelStart = EditEntryTunnelDialog.TunnelStart;
    entry.Model.TunnelEnd = EditEntryTunnelDialog.TunnelEnd;

    entry.Screen.UpdateEntries();
  }
}
