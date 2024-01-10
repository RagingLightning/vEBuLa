using Microsoft.Extensions.Logging;
using System.Linq;
using vEBuLa.Models;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class AddEbulaEntryC : BaseC {
  private ILogger<AddEbulaEntryC>? Logger => App.GetService<ILogger<AddEbulaEntryC>>();
  private readonly EbulaVM Ebula;

  internal AddEbulaEntryC(EbulaVM ebula) {
    Ebula = ebula;
  }

  public override void Execute(object? parameter) {
    var newEntry = new EbulaEntry();

    if (parameter is EbulaMarkerEntryVM marker) {
      Logger?.LogInformation("Adding new entry above {ExistingMarker}", marker);
      Logger?.LogDebug("{MarkerEntry} is marker of type {MarkerType}", marker, marker.MarkerType);
      if (marker.MarkerType == EbulaMarkerType.PRE) {
        Logger?.LogTrace("Inserting new entry {NewEntry} in PreEntries of segment {Segment} at index {Index}", newEntry, marker.Segment, 0);
        marker.Segment.Model.PreEntries.Insert(0, newEntry);
      }
      else if (marker.MarkerType == EbulaMarkerType.MAIN) {
        Logger?.LogTrace("Inserting new entry {NewEntry} in Entries of segment {Segment} at index {Index}", newEntry, marker.Segment, 0);
        marker.Segment.Model.Entries.Insert(0, newEntry);
      }
      else if (marker.MarkerType == EbulaMarkerType.POST) {
        Logger?.LogTrace("Inserting new entry {NewEntry} in PostEntries of segment {Segment} at index {Index}", newEntry, marker.Segment, 0);
        marker.Segment.Model.PostEntries.Insert(0, newEntry);
      }

      marker.Screen.StartEntry += 1;
      marker.Screen.UpdateEntries();
      marker.Screen.Ebula.MarkDirty();
    }
    else if (parameter is EbulaEntryVM entry) {
      Logger?.LogInformation("Adding new entry above {ExistingEntry}", entry);
      var index = Ebula.Model.Segments.Select(s => s.FindEntry(entry.Model)).FirstOrDefault(p => p is not null);
      if (index is null) {
        Logger?.LogWarning("Unable to locate {ExistingEntry} in loaded sequence", entry);
        return;
      }
      Logger?.LogTrace("Inserting new entry {NewEntry} in {Entries} at index {Index}", newEntry, index.Value.List, index.Value.Index);
      index.Value.List.Insert(index.Value.Index+1, newEntry);

      entry.Screen.StartEntry += 1;
      entry.Screen.UpdateEntries();
      entry.Screen.Ebula.MarkDirty();
    }
    else return;
  }
}
