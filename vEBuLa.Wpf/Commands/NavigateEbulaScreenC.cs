using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using vEBuLa.Models;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands;
internal class NavigateEbulaScreenC : NavigateScreenC {
  private ILogger<NavigateEbulaScreenC>? Logger => App.GetService<ILogger<NavigateEbulaScreenC>>();
  private readonly EbulaScreenVM Screen;

  private EbulaNavState State = EbulaNavState.DEFAULT;

  public NavigateEbulaScreenC(EbulaScreenVM screen) {
    Screen = screen;
    SetNavState(EbulaNavState.DEFAULT);
  }

  protected override void Cancel() {
    Logger?.LogTrace("Ebula Navigation: {Button} - {Action}", "Cancel", "Reset Start entry");
    Screen.StartEntry = 0;
    Screen.CurrentEntry = 0;
  }

  protected override void Accept() {
    Logger?.LogTrace("Ebula Navigation: {Button} - {Action}", "Enter", "Jump to next Departure");
    var index = Screen.Ebula.EditMode ? Screen.StartEntry : Screen.CurrentEntry;
    try {
      var targetEntry = Screen.Entries.Skip(index + 1).First(e => e is EbulaEntryVM entry && (entry.Arrival is not null || entry.Departure is not null));

      if (Screen.Ebula.EditMode) Screen.StartEntry = Screen.Entries.IndexOf(targetEntry);
      else Screen.CurrentEntry = Screen.Entries.IndexOf(targetEntry);
    }
    catch (Exception) { }
  }

  protected override void MoveDown() {
    Logger?.LogTrace("Ebula Navigation: {Button} - {Action}", "Down", "Move back one entry");
    if (Screen.Ebula.EditMode) Screen.StartEntry -= 1;
    else Screen.CurrentEntry -= 1;
  }

  protected override void MoveLeft() {
    Logger?.LogTrace("Ebula Navigation: {Button} - {Action}", "Left", "Move back one page");
    if (Screen.Ebula.EditMode) Screen.StartEntry -= 15;
    else Screen.CurrentEntry -= 15;
  }

  protected override void MoveRight() {
    Logger?.LogTrace("Ebula Navigation: {Button} - {Action}", "Right", "Move forward one page");
    if (Screen.Ebula.EditMode) Screen.StartEntry += 15;
    else Screen.CurrentEntry += 15;
  }

  protected override void MoveUp() {
    Logger?.LogTrace("Ebula Navigation: {Button} - {Action}", "Up", "Move forward one entry");
    if (Screen.Ebula.EditMode) Screen.StartEntry += 1;
    else Screen.CurrentEntry += 1;
  }

  protected override void St() {
    Screen.Ebula.Screen = new SetupScreenVM(Screen.Ebula);
    Screen.Destroy();
  }

  protected override void Button6() {
    switch (State) {
      case EbulaNavState.DEFAULT: 
        SetNavState(EbulaNavState.CONTROL_MODE_SETTINGS);
        Screen.ControlModeOpen = true;
        return;
    }
  }

  private void SetNavState(EbulaNavState state) {
    State = state;
    switch (state) {
      case EbulaNavState.DEFAULT:
        Screen.ButtonLabel0 = "Zug";
        Screen.ButtonLabel1 = "FSD";
        Screen.ButtonLabel2 = null;
        Screen.ButtonLabel3 = null;
        Screen.ButtonLabel4 = "LW";
        Screen.ButtonLabel5 = "GW";
        Screen.ButtonLabel6 = "Zeit";
        Screen.ButtonLabel7 = null;
        Screen.ButtonLabel8 = null;
        Screen.ButtonLabel9 = "G";
        return;
      case EbulaNavState.CONTROL_MODE_SETTINGS:
        Screen.ButtonLabel0 = "1";
        Screen.ButtonLabel1 = "2";
        Screen.ButtonLabel2 = "3";
        Screen.ButtonLabel3 = "4";
        Screen.ButtonLabel4 = "5";
        Screen.ButtonLabel5 = "6";
        Screen.ButtonLabel6 = "7";
        Screen.ButtonLabel7 = "8";
        Screen.ButtonLabel8 = "9";
        Screen.ButtonLabel9 = "0";
        return;

    }
  }
}

internal enum EbulaNavState {
  DEFAULT,
  CONTROL_MODE_SETTINGS
}
