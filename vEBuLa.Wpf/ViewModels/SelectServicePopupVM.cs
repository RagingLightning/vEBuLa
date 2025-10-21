using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using vEBuLa.Commands.Navigation;
using vEBuLa.Extensions;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;

internal class SelectServicePopupVM : BaseVM {
  EbulaScreenVM Screen;
  public ObservableCollection<EbulaServiceVM> Services { get; } = new();

  private EbulaServiceVM _selectedService;
  public EbulaServiceVM SelectedService {
    get {
      return _selectedService;
    }
    set {
      _selectedService = value;
      OnPropertyChanged(nameof(SelectedService));
    }
  }
  public int SelectedServiceIndex { get; set; }

  public SelectServicePopupVM(EbulaScreenVM screen, IEnumerable<EbulaServiceVM> services) {
    Screen = screen;
    foreach (var service in services) { service.Selected = false; }
    Services.AddRange(services);
    SelectedService = Services[0];
    SelectedService.Selected = true;

    Screen.Ebula.NavigateCommand = new NavigateSelectServicePopupC(screen, this);
  }

  internal void Close(bool accept) {
    if (accept)
      Screen.Ebula.LoadService(SelectedService.Model);
    else
      Screen.PopupWindow = null;
  }

  internal void Previous() {
    SelectedService.Selected = false;
    SelectedService = Services[Math.Max(SelectedServiceIndex - 1, 0)];
    SelectedService.Selected = true;
  }

  internal void Next() {
    SelectedService.Selected = false;
    SelectedService = Services[Math.Min(SelectedServiceIndex + 1, Services.Count - 1)];
    SelectedService.Selected = true;
  }
}