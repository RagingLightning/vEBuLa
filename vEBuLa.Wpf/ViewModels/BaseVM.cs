using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace vEBuLa.ViewModels;
public abstract class BaseVM : INotifyPropertyChanged {
  private ILogger<BaseVM>? Logger = App.GetService<ILogger<BaseVM>>();
  public event PropertyChangedEventHandler? PropertyChanged;

  protected void OnPropertyChanged(string propertyName) {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

  public virtual void Destroy() {}

  ~BaseVM() {
    Logger?.LogTrace("Destroy: {Type} {Instance}", this.GetType().FullName, this);
  }

}
