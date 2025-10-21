using Microsoft.Extensions.Logging;
using System.Linq;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class RemoveEbulaEntryC : BaseC {
  internal static readonly RemoveEbulaEntryC INSTANCE = new();

  private ILogger<RemoveEbulaEntryC>? Logger => App.GetService<ILogger<RemoveEbulaEntryC>>();

  public override void Execute(object? parameter) {
    if (parameter is not EbulaEntryVM entry) return;
    if (entry.Screen.Ebula.Service is null) return;
    Logger?.LogInformation("Removing entry {EbulaEntry}", entry);

    var index = entry.Screen.Ebula.Service.Segments.Select(s => s.FindEntry(entry.Model)).FirstOrDefault(p => p is not null);
    if (index is null) {
      Logger?.LogWarning("Unable to locate {EbulaEntry} in loaded sequence", entry);
      return;
    }

    index.Value.List.Remove(entry.Model);

    entry.Screen.UpdateEntries();
    entry.Screen.Ebula.MarkDirty();
  }
}
