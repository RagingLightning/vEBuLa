using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using vEBuLa.Models;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class LoadEbulaConfigC : BaseC {
  private ILogger<LoadEbulaConfigC>? Logger => App.GetService<ILogger<LoadEbulaConfigC>>();
  private StorageConfigScreenVM Screen { get; }

  public LoadEbulaConfigC(StorageConfigScreenVM screen) {
    Screen = screen;
  }

  public override void Execute(object? parameter) {
    var dialog = new OpenFileDialog {
      FileName = "vEBuLa",
      DefaultExt = ".json",
      Filter = "vEBuLa config files|*.json|All Files|*.*"
    };

    if (dialog.ShowDialog() != true) return;

    try {
      Screen.Ebula.Model = new Ebula(dialog.FileName);
    } catch (Exception ex) {
      Logger?.LogError(ex, "Failed to load Config from {FileName}", dialog.FileName);
    }

    if (Screen.Ebula.Model is not null)
      Screen.LoadConfig();

  }
}
