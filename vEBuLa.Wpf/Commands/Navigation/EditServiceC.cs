using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Commands;
using vEBuLa.Dialogs;
using vEBuLa.Models;
using vEBuLa.ViewModels;

namespace vEBuLa;

internal class EditServiceC : BaseC {
  private readonly ILogger<EditServiceC>? Logger = App.GetService<ILogger<EditServiceC>>();
  private readonly SetupScreenVM Screen;

  public EditServiceC(SetupScreenVM screen) {
    Screen = screen;
  }

  public override void Execute(object? parameter) {
    if (!Screen.Ebula.EditMode) return;
    if (Screen.Ebula.Model.Config is null) return;
    if (parameter is not EbulaServiceVM serviceVM) return;
    if (serviceVM.Model is not EbulaService service) return;

    var mainWindow = Application.Current.MainWindow;
    var dialog = new EditServiceDialog(serviceVM.Name, serviceVM.Description, serviceVM.StartTime, serviceVM.Vehicles, mainWindow.PointToScreen(Mouse.GetPosition(mainWindow)) - new Point(150, 20));

    if (dialog.ShowDialog() == false) {
      Logger?.LogDebug("{EditType} edit aborted by user", "Service");
      return;
    }

    if (EditServiceDialog.Delete) {
      Screen.Ebula.Model.Config.Services.Remove(service.Id);
      Screen.RouteServices.Remove(serviceVM);
      Screen.Ebula.MarkDirty();
    }

    serviceVM.Name = EditServiceDialog.ServiceName;
    serviceVM.Description = EditServiceDialog.Description;
    serviceVM.StartTime = EditServiceDialog.StartTime;
    serviceVM.Vehicles = EditServiceDialog.Vehicle;
    Screen.Ebula.MarkDirty();
  }
}