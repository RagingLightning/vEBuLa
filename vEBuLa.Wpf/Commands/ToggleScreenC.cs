using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;

internal class ToggleScreenC : BaseC {
  private ILogger<ToggleScreenC>? Logger => App.AppHost?.Services.GetRequiredService<ILogger<ToggleScreenC>>();
  private readonly EbulaScreenVM Screen;

  public ToggleScreenC(EbulaScreenVM screen) { Screen = screen; }

  public override void Execute(object? parameter) {
    Screen.Active = !Screen.Active;

    if (Screen.Active) Logger?.LogInformation("Screen activated");
    else Logger?.LogInformation("Screen deactivated");
  }
}
