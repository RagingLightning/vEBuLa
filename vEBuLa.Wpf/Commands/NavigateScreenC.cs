namespace vEBuLa.Commands;
abstract partial class NavigateScreenC : BaseC {
  public override void Execute(object? parameter) {
    switch (parameter) {
      case NavAction.CANCEL: Cancel(); return;
      case NavAction.MOVE_UP: MoveUp(); return;
      case NavAction.MOVE_DOWN: MoveDown(); return;
      case NavAction.MOVE_LEFT: MoveLeft(); return;
      case NavAction.MOVE_RIGHT: MoveRight(); return;
      case NavAction.ACCEPT: Accept(); return;
    }
  }

  protected abstract void Accept();
  protected abstract void MoveRight();
  protected abstract void MoveLeft();
  protected abstract void MoveDown();
  protected abstract void MoveUp();
  public abstract void Cancel();
}

public enum NavAction {
  CANCEL,
  MOVE_UP,
  MOVE_DOWN,
  MOVE_LEFT,
  MOVE_RIGHT,
  ACCEPT
}