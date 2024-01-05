using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Windows.Controls;
using vEBuLa.Models;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class AddEbulaEntryC : BaseC {
  private ILogger<AddEbulaEntryC>? logger;
  private readonly EbulaVM Ebula;

  internal AddEbulaEntryC(EbulaVM ebula) {
    logger = App.GetService<ILogger<AddEbulaEntryC>>();

    Ebula = ebula;
  }

  public override void Execute(object? parameter) {
    var newEntry = new EbulaEntry();

    if (parameter is EbulaMarkerEntryVM marker) {
      logger?.LogInformation("Adding new entry above {ExistingMarker}", marker);
      logger?.LogDebug("{MarkerEntry} is marker of type {MarkerType}", marker, marker.MarkerType);
      if (marker.MarkerType == EbulaMarkerType.PRE) {
        logger?.LogTrace("Inserting new entry {NewEntry} in PreEntries of segment {Segment} at index {Index}", newEntry, marker.Segment, 0);
        marker.Segment.PreEntries.Insert(0, newEntry);
      }
      else if (marker.MarkerType == EbulaMarkerType.MAIN) {
        logger?.LogTrace("Inserting new entry {NewEntry} in Entries of segment {Segment} at index {Index}", newEntry, marker.Segment, 0);
        marker.Segment.Entries.Insert(0, newEntry);
      }
      else if (marker.MarkerType == EbulaMarkerType.POST) {
        logger?.LogTrace("Inserting new entry {NewEntry} in PostEntries of segment {Segment} at index {Index}", newEntry, marker.Segment, 0);
        marker.Segment.PostEntries.Insert(0, newEntry);
      }
    }
    else if (parameter is EbulaEntryVM entry) {
      logger?.LogInformation("Adding new entry above {ExistingEntry}", entry);
      var index = Ebula.Model.Segments.Select(s => s.FindEntry(entry.Model)).FirstOrDefault(p => p is not null);
      if (index is null) {
        logger?.LogWarning("Unable to locate {ExistingEntry} in loaded sequence", entry);
        return;
      }
      logger?.LogTrace("Inserting new entry {NewEntry} in {Entries} at index {Index}", newEntry, index.Value.List, index.Value.Index);
      index.Value.List.Insert(index.Value.Index, newEntry);
    }
    else return;

    Ebula.Screen.EditEntryCommand.Execute(newEntry);
  }
}
