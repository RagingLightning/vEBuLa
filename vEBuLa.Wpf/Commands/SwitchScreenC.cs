using Microsoft.Extensions.Logging;
using System;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class SwitchScreenC : BaseC {
  private ILogger<SwitchScreenC>? Logger => App.GetService<ILogger<SwitchScreenC>>();

  private EbulaVM Ebula;
  private Func<ScreenBaseVM?> Constructor;

  public SwitchScreenC(EbulaVM ebula, Func<ScreenBaseVM?> constructor) {
    Ebula = ebula;
    Constructor = constructor;
  }

  public override void Execute(object? parameter) {
    var screen = Constructor();
    if (screen is null) return;
    Logger?.LogInformation("Switching to new screen of Type {ScreenType}", screen.GetType());
    Ebula.Screen = screen;
  }
}
