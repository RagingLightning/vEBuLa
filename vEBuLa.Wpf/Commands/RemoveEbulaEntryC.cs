using Microsoft.Extensions.Logging;
using System.Linq;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class RemoveEbulaEntryC : BaseC {
  private ILogger<RemoveEbulaEntryC>? Logger => App.GetService<ILogger<RemoveEbulaEntryC>>();
  private readonly EbulaVM Ebula;

  internal RemoveEbulaEntryC(EbulaVM ebula) {    
    Ebula = ebula;
  }

  public override void Execute(object? parameter) {
    if (parameter is not EbulaEntryVM entry) return;
    Logger?.LogInformation("Removing entry {EbulaEntry}", entry);

    var index = Ebula.Model.Segments.Select(s => s.FindEntry(entry.Model)).FirstOrDefault(p => p is not null);
    if (index is null) {
      Logger?.LogWarning("Unable to locate {EbulaEntry} in loaded sequence", entry);
      return;
    }

    index.Value.List.Remove(entry.Model);

    Ebula.Screen.UpdateEntries();
  }
}
