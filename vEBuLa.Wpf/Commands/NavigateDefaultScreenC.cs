using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class NavigateDefaultScreenC : NavigateScreenC {
  private ILogger<NavigateDefaultScreenC>? Logger => App.AppHost?.Services.GetRequiredService<ILogger<NavigateDefaultScreenC>>();
  private readonly EbulaScreenVM Screen;

  public NavigateDefaultScreenC(EbulaScreenVM screen) { Screen = screen; }
  public override void Cancel() {
    Screen.StartEntry = 0;
  }

  protected override void Accept() {
    var targetEntry = Screen.Entries.Skip(Screen.StartEntry + 1).First(e => e.Arrival is not null || e.Departure is not null);
    Screen.StartEntry = Screen.Entries.IndexOf(targetEntry);
  }

  protected override void MoveDown() {
    Screen.StartEntry -= 1;
  }

  protected override void MoveLeft() {
    Screen.StartEntry -= 15;
  }

  protected override void MoveRight() {
    Screen.StartEntry += 15;
  }

  protected override void MoveUp() {
    Screen.StartEntry += 1;
  }
}
