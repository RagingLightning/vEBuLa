using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using vEBuLa.Commands.Navigation;
using vEBuLa.Extensions;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;

internal class SelectConfigPopupVM : BaseVM {
  EbulaScreenVM Screen;
  public ObservableCollection<EbulaConfigVM> Configs { get; } = new();

  private EbulaConfigVM _selectedConfig;
  public EbulaConfigVM SelectedConfig {
    get {
      return _selectedConfig;
    }
    set {
      _selectedConfig = value;
      OnPropertyChanged(nameof(SelectedConfig));
    }
  }
  public int SelectedConfigIndex { get; set; }

  public SelectConfigPopupVM(EbulaScreenVM screen, IEnumerable<EbulaConfigVM> configs) {
    Screen = screen;
    Configs.AddRange(configs);
    SelectedConfig = Configs[0];
    SelectedConfig.Selected = true;

    Screen.Ebula.NavigateCommand = new NavigateSelectConfigPopupC(screen, this);
  }

  internal void Close(bool accept) {
    if (accept)
      Screen.PopupWindow = new SelectVehiclePopupVM(Screen, SelectedConfig);
    else
      Screen.PopupWindow = null;
  }

  internal void Previous() {
    SelectedConfig.Selected = false;
    SelectedConfig = Configs[Math.Max(SelectedConfigIndex - 1, 0)];
    SelectedConfig.Selected = true;
  }

  internal void Next() {
    SelectedConfig.Selected = false;
    SelectedConfig = Configs[Math.Min(SelectedConfigIndex + 1, Configs.Count - 1)];
    SelectedConfig.Selected = true;
  }
}