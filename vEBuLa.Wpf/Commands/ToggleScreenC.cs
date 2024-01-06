using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;

internal class ToggleScreenC : BaseC {
  private ILogger<ToggleScreenC>? Logger => App.AppHost?.Services.GetRequiredService<ILogger<ToggleScreenC>>();
  private readonly EbulaVM Ebula;

  public ToggleScreenC(EbulaVM ebula) { Ebula = ebula; }

  public override void Execute(object? parameter) {
    Ebula.Active = !Ebula.Active;

    if (Ebula.Active) Logger?.LogInformation("Screen activated");
    else Logger?.LogInformation("Screen deactivated");
  }
}
