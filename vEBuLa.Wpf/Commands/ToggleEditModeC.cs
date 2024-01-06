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
  private readonly EbulaVM Ebula;
  public ToggleEditModeC(EbulaVM ebula) { Ebula = ebula; }
  public override void Execute(object? parameter) {
    Ebula.EditMode = !Ebula.EditMode;

    if (Ebula.EditMode) Logger?.LogInformation("Edit Mode activated");
    else Logger?.LogInformation("Edit Mode deactivated");
  }
}
