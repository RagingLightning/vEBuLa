namespace vEBuLa.Commands;
abstract partial class NavigateScreenC : BaseC {
  public override void Execute(object? parameter) {
    switch (parameter) {
      /* Top Side */
      case NavAction.S: S(); return;
      case NavAction.I: I(); return;
      case NavAction.ST: St(); return;
      case NavAction.POSITIVE_SPEED: PositiveSpeed(); return;
      case NavAction.ZERO_SPEED: ZeroSpeed(); return;
      case NavAction.BRIGHTNESS: Brightness(); return;
      case NavAction.CONTRAST: Contrast(); return;

      /* Right Side */
      case NavAction.CANCEL: Cancel(); return;
      case NavAction.MOVE_UP: MoveUp(); return;
      case NavAction.MOVE_DOWN: MoveDown(); return;
      case NavAction.MOVE_LEFT: MoveLeft(); return;
      case NavAction.MOVE_RIGHT: MoveRight(); return;
      case NavAction.ACCEPT: Accept(); return;

      /* Bottom Side */
      case NavAction.BUTTON0: Button0(); return;
      case NavAction.BUTTON1: Button1(); return;
      case NavAction.BUTTON2: Button2(); return;
      case NavAction.BUTTON3: Button3(); return;
      case NavAction.BUTTON4: Button4(); return;
      case NavAction.BUTTON5: Button5(); return;
      case NavAction.BUTTON6: Button6(); return;
      case NavAction.BUTTON7: Button7(); return;
      case NavAction.BUTTON8: Button8(); return;
      case NavAction.BUTTON9: Button9(); return;
    }
  }

  /* Top Side */
  // OnOff handled by EBuLaVM -> ToggleScreenC
  // UD handled by EBuLaVM    -> ToggleEditC
  protected virtual void S() { }
  protected virtual void I() { }
  protected virtual void St() { }
  protected virtual void PositiveSpeed() { }
  protected virtual void ZeroSpeed() { }
  protected virtual void Brightness() { }
  protected virtual void Contrast() { }

  /* Right Side */
  protected virtual void Accept() { }
  protected virtual void MoveRight() { }
  protected virtual void MoveLeft() { }
  protected virtual void MoveDown() { }
  protected virtual void MoveUp() { }
  protected virtual void Cancel() { }

  /* Bottom Side */
  protected virtual void Button0() { }
  protected virtual void Button1() { }
  protected virtual void Button2() { }
  protected virtual void Button3() { }
  protected virtual void Button4() { }
  protected virtual void Button5() { }
  protected virtual void Button6() { }
  protected virtual void Button7() { }
  protected virtual void Button8() { }
  protected virtual void Button9() { }
}

public enum NavAction {
  /* Top Side */
  S,
  I,
  ST,
  POSITIVE_SPEED,
  ZERO_SPEED,
  BRIGHTNESS,
  CONTRAST,

  /* Right Side */
  CANCEL,
  MOVE_UP,
  MOVE_DOWN,
  MOVE_LEFT,
  MOVE_RIGHT,
  ACCEPT,

  /* Bottom Side */
  BUTTON0,
  BUTTON1,
  BUTTON2,
  BUTTON3,
  BUTTON4,
  BUTTON5,
  BUTTON6,
  BUTTON7,
  BUTTON8,
  BUTTON9,
}