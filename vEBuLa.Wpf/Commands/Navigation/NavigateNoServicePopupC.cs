using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands.Navigation;
internal class NavigateNoServicePopupC : NavigateScreenC {
  EbulaScreenVM Screen;

  public NavigateNoServicePopupC(EbulaScreenVM screen) {
    Screen = screen;
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

  protected override void Accept() {
    Screen.PopupWindow = null;
  }

}
