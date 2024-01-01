using vEBuLa.Commands;

namespace vEBuLa.ViewModels;
public abstract class ScreenBaseVM : BaseVM {
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
  #endregion
}
