using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands.Navigation;
internal class NavigateServiceNumberPopupC : NavigateScreenC {
  ServiceNumberPopupVM Popup;

  public NavigateServiceNumberPopupC(ServiceNumberPopupVM popup, EbulaScreenVM screen) {
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

  protected override void Accept() {
    Popup.Accept();
  }

  protected override void MoveRight() {
    Popup.Overview();
  }

  protected override void Cancel() {
    Popup.Cancel();
  }

  protected override void Button0() {
    Popup.ServiceNumber += 1;
  }

  protected override void Button1() {
    Popup.ServiceNumber += 2;
  }

  protected override void Button2() {
    Popup.ServiceNumber += 3;
  }

  protected override void Button3() {
    Popup.ServiceNumber += 4;
  }

  protected override void Button4() {
    Popup.ServiceNumber += 5;
  }

  protected override void Button5() {
    Popup.ServiceNumber += 6;
  }

  protected override void Button6() {
    Popup.ServiceNumber += 7;
  }

  protected override void Button7() {
    Popup.ServiceNumber += 8;
  }

  protected override void Button8() {
    Popup.ServiceNumber += 9;
  }

  protected override void Button9() {
    Popup.ServiceNumber += 0;
  }

}
