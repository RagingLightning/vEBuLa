using vEBuLa.Commands;

namespace vEBuLa.ViewModels;
internal abstract class ScreenBaseVM : BaseVM {
  public EbulaVM Ebula { get; }

  protected ScreenBaseVM(EbulaVM ebula) {
    Ebula = ebula;
    Ebula.PropertyChanged += Ebula_PropertyChanged;
  }

  private void Ebula_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
    if (sender != Ebula) return;
    if (e.PropertyName == nameof(Ebula.EditMode)) OnPropertyChanged(nameof(EditMode));
  }

  #region Properties
  #region Commands
  private BaseC _navigateCommand;
  public BaseC NavigateCommand {
    get {
      return _navigateCommand;
    }
    protected set {
      _navigateCommand = value;
      OnPropertyChanged(nameof(NavigateCommand));
    }
  }
  #endregion

  public bool EditMode => Ebula.EditMode;

  public bool NormalMode => Ebula.NormalMode;

  #endregion
}
