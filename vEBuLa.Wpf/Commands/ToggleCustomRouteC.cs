using Microsoft.Extensions.Logging;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class ToggleCustomRouteC : BaseC {
  private ILogger<ToggleCustomRouteC>? Logger => App.GetService<ILogger<ToggleCustomRouteC>>();
  private StorageConfigScreenVM Screen;

  public ToggleCustomRouteC(StorageConfigScreenVM screen) {
    Screen = screen;
  }

  public override void Execute(object? parameter) {
    if (Screen.UsingRoutes) Logger?.LogInformation("Switching to custom route creation");
    else Logger?.LogInformation("Switching to predefined route");

    Screen.UsingRoutes = !Screen.UsingRoutes;
  }
}
