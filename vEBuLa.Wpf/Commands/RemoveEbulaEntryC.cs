using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class RemoveEbulaEntryC : BaseC {
  private ILogger<RemoveEbulaEntryC>? Logger => App.AppHost?.Services.GetRequiredService<ILogger<RemoveEbulaEntryC>>();
  private readonly EbulaVM Ebula;

  internal RemoveEbulaEntryC(EbulaVM ebula) { Ebula = ebula; }

  public override void Execute(object? parameter) {
    if (parameter is not EbulaEntryVM entry) return;
    Logger?.LogInformation("Removing entry {ExistingEntry}", entry);

    Ebula.Model.RemoveEntry(entry.Model);

    Ebula.Screen.UpdateEntries();
  }
}
