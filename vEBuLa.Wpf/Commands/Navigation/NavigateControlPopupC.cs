using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.ViewModels;

namespace vEBuLa.Commands.Navigation;
class NavigateControlPopupC : NavigateScreenC {
  private EbulaControlPopupVM Popup;
  public NavigateControlPopupC(EbulaControlPopupVM popup, EbulaScreenVM screen) {
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
    Popup.Close(true);
  }

  protected override void Cancel() {
    Popup.Close(false);
  }

  protected override void MoveRight() {
    Popup.NextGroup();
  }

  protected override void MoveDown() {
    Popup.NextSetting();
  }

  protected override void MoveUp() {
    Popup.PrevSetting();
  }

  protected override void Button0() {
    Popup.AppendDigit(1);
  }

  protected override void Button1() {
    Popup.AppendDigit(2);
  }

  protected override void Button2() {
    Popup.AppendDigit(3);
  }

  protected override void Button3() {
    Popup.AppendDigit(4);
  }

  protected override void Button4() {
    Popup.AppendDigit(5);
  }

  protected override void Button5() {
    Popup.AppendDigit(6);
  }

  protected override void Button6() {
    Popup.AppendDigit(7);
  }

  protected override void Button7() {
    Popup.AppendDigit(8);
  }

  protected override void Button8() {
    Popup.AppendDigit(9);
  }

  protected override void Button9() {
    Popup.AppendDigit(0);
  }
}