using Microsoft.Extensions.Logging;
using System;
using System.Windows.Input;

namespace vEBuLa.Commands;

public abstract class BaseC : ICommand {
  private ILogger<BaseC>? Logger => App.GetService<ILogger<BaseC>>();
  public event EventHandler? CanExecuteChanged;

  public virtual bool CanExecute(object? parameter) {
    return true;
  }

  public abstract void Execute(object? parameter);

  protected void OnCanExecuteChanged() {
    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
  }

  public virtual void Destroy() {}

  ~BaseC() {
    Logger?.LogTrace("Destroy: {Type} {Instance}", this.GetType().FullName, this);
  }
}
