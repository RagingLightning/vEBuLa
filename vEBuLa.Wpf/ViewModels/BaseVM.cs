using System.ComponentModel;

namespace vEBuLa.ViewModels;
public abstract class BaseVM : INotifyPropertyChanged {
  public event PropertyChangedEventHandler? PropertyChanged;

  protected void OnPropertyChanged(string propertyName) {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}
