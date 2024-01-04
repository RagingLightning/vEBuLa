using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class ToggleEditModeC : BaseC {
  private ILogger<ToggleEditModeC>? Logger => App.AppHost?.Services.GetRequiredService<ILogger<ToggleEditModeC>>();
  private readonly EbulaScreenVM Screen;
  public ToggleEditModeC(EbulaScreenVM screen) { Screen = screen; }
  public override void Execute(object? parameter) {
    Screen.EditMode = !Screen.EditMode;

    if (Screen.EditMode) Logger?.LogInformation("Edit Mode activated");
    else Logger?.LogInformation("Edit Mode deactivated");
  }
}
