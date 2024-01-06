using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using vEBuLa.Models;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class SaveEbulaConfigC : BaseC {
  private ILogger<SaveEbulaConfigC>? Logger => App.GetService<ILogger<SaveEbulaConfigC>>();
  private StorageConfigScreenVM Screen { get; }

  public SaveEbulaConfigC(StorageConfigScreenVM screen) {
    Screen = screen;
  }

  public override void Execute(object? parameter) {
    var dialog = new SaveFileDialog {
      FileName = "vEBuLa",
      DefaultExt = ".json",
      Filter = "vEBuLa config files|*.json|All Files|*.*"
    };

    if (dialog.ShowDialog() != true) return;

    try {
      Screen.Ebula.Model.Config?.Save(dialog.FileName);
    } catch (Exception ex) {
      Logger?.LogError(ex, "Failed to save Config to {FileName}", dialog.FileName);
    }

  }
}
