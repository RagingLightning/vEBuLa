using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.Wpf.Commands;

namespace vEBuLa.Wpf.ViewModels;
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
