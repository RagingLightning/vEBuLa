using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.Models;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class EditEbulaEntryC : BaseC {
  private ILogger<EditEbulaEntryC>? Logger => App.AppHost?.Services.GetRequiredService<ILogger<EditEbulaEntryC>>();
  private EbulaScreenVM Screen;

  public EditEbulaEntryC(EbulaScreenVM screen) { Screen = screen; }

  public override void Execute(object? parameter) {
    if (parameter is EbulaEntryVM entryVM) parameter = entryVM.Model;
    if (parameter is not EbulaEntry entry) return;
    Logger?.LogInformation("Editing entry {ExistingEntry}", entry);

    // TODO: Actual UI
    Console.Write("Name: ");
    entry.LocationName = Console.ReadLine();

    Screen.UpdateEntries();
  }
}
