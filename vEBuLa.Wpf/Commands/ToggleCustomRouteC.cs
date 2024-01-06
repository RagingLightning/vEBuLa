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
    Screen.UsingRoutes = !Screen.UsingRoutes;

    if (Screen.UsingRoutes) Logger?.LogInformation("Using defined routes");
    else Logger?.LogInformation("Using custom Routes");
  }
}
