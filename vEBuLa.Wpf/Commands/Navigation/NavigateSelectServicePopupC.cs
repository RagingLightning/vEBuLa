using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands.Navigation;
internal class NavigateSelectServicePopupC : NavigateScreenC {
  EbulaScreenVM Screen;
  SelectServicePopupVM Popup;

  public NavigateSelectServicePopupC(EbulaScreenVM screen, SelectServicePopupVM popup) {
    Screen = screen;
    Popup = popup;
    screen.ButtonLabel0 = "1";
    screen.ButtonLabel1 = "2";
    screen.ButtonLabel2 = "3";
    screen.ButtonLabel3 = "4";
    screen.ButtonLabel4 = "5";
    screen.ButtonLabel5 = "6";
    screen.ButtonLabel6 = "7";
    screen.ButtonLabel7 = "8";
    screen.ButtonLabel8 = "9";
    screen.ButtonLabel9 = "0";
  }

  protected override void Cancel() {
    Popup.Close(false);
  }

  protected override void Accept() {
    Popup.Close(true);
  }

  protected override void MoveUp() {
    Popup.Previous();
  }

  protected override void MoveDown() {
    Popup.Next();
  }

}
