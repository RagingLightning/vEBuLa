using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class RemoveEbulaEntryC : BaseC {
  private ILogger<RemoveEbulaEntryC>? logger;
  private readonly EbulaVM Ebula;

  internal RemoveEbulaEntryC(EbulaVM ebula) {
    logger = App.GetService<ILogger<RemoveEbulaEntryC>>();
    
    Ebula = ebula;
  }

  public override void Execute(object? parameter) {
    if (parameter is not EbulaEntryVM entry) return;
    if (entry is EbulaMarkerEntryVM) return;
    logger?.LogInformation("Removing entry {ExistingEntry}", entry);

    var index = Ebula.Model.Segments.Select(s => s.FindEntry(entry.Model)).FirstOrDefault(p => p is not null);
    if (index is null) {
      logger?.LogWarning("Unable to locate {ExistingEntry} in loaded sequence", entry);
      return;
    }

    index.Value.List.Remove(entry.Model);

    Ebula.Screen.UpdateEntries();
  }
}
