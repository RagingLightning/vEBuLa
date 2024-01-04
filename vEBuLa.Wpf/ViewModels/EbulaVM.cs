using System;
using System.ComponentModel;
using vEBuLa.Commands;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;
internal class EbulaVM : BaseVM {
  public Ebula Model { get; private set; }

  public EbulaVM(Ebula ebula) {
    Model = ebula;
    Screen = new EbulaScreenVM(this);

    Screen.PropertyChanged += Screen_NavigateCommandChanged;

    ToggleScreenCommand = new ToggleScreenC(Screen);
    ToggleEditCommand = new ToggleEditModeC(Screen);
  }

  // Propagate change of Navigation command
  private void Screen_NavigateCommandChanged(object? sender, PropertyChangedEventArgs e) {
    if (sender != Screen) return;
    OnPropertyChanged(nameof(NavigateCommand));
  }

  #region Properties

  private EbulaScreenVM _screen;
  public EbulaScreenVM Screen {
    get {
      return _screen;
    }
    private set {
      _screen = value;
      OnPropertyChanged(nameof(Screen));
    }
  }

  #region Commands

  public BaseC ToggleEditCommand { get; }

  public BaseC ToggleScreenCommand { get; }

  public BaseC NavigateCommand => Screen.NavigateCommand; //managed by the screen instance
  #endregion

  #endregion

}
