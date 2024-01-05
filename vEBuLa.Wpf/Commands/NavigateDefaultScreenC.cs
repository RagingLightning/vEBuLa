using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class NavigateDefaultScreenC : NavigateScreenC {
  private ILogger<NavigateDefaultScreenC>? Logger => App.GetService<ILogger<NavigateDefaultScreenC>>();
  private readonly EbulaScreenVM Screen;

  public NavigateDefaultScreenC(EbulaScreenVM screen) { Screen = screen; }
  public override void Cancel() {
    Logger?.LogTrace("Ebula Navigation: {Button} - {Action}", "Cancel", "Reset Start entry");
    Screen.StartEntry = 0;
  }

  protected override void Accept() {
    Logger?.LogTrace("Ebula Navigation: {Button} - {Action}", "Enter", "Jump to next Departure");
    var targetEntry = Screen.Entries.Skip(Screen.StartEntry + 1).First(e =>  e is EbulaEntryVM entry && (entry.Arrival is not null || entry.Departure is not null));
    Screen.StartEntry = Screen.Entries.IndexOf(targetEntry);
  }

  protected override void MoveDown() {
    Logger?.LogTrace("Ebula Navigation: {Button} - {Action}", "Down", "Move back one entry");
    Screen.StartEntry -= 1;
  }

  protected override void MoveLeft() {
    Logger?.LogTrace("Ebula Navigation: {Button} - {Action}", "Left", "Move back one page");
    Screen.StartEntry -= 15;
  }

  protected override void MoveRight() {
    Logger?.LogTrace("Ebula Navigation: {Button} - {Action}", "Right", "Move forward one page");
    Screen.StartEntry += 15;
  }

  protected override void MoveUp() {
    Logger?.LogTrace("Ebula Navigation: {Button} - {Action}", "Up", "Move forward one entry");
    Screen.StartEntry += 1;
  }
}
