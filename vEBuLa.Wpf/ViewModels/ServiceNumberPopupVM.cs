using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.Commands.Navigation;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;
internal class ServiceNumberPopupVM : BaseVM {
  public EbulaScreenVM Screen { get; }
  Ebula Ebula;

  public ServiceNumberPopupVM(EbulaScreenVM screen, Ebula ebula) {
    Screen = screen;
    Ebula = ebula;

    screen.Ebula.NavigateCommand = new NavigateServiceNumberPopupC(this, screen);
  }

  public void Accept() {
    if (ServiceNumber == string.Empty)
      ServiceNumber = "0";

    var matchingServices = Ebula.LoadedConfigs.Select(c => c.Services.Where(s => s.Value.Number == ServiceNumber).Select(s => s.Value)).Aggregate((a, b) => a.Concat(b));
    if (matchingServices.Count() == 0)
      Screen.PopupWindow = new NoServicePopupVM(Screen, ServiceNumber);
    //else if (matchingServices.Count() == 1)
    //  Screen.Ebula.LoadService(matchingServices.First());
    else
      Screen.PopupWindow = new SelectServicePopupVM(Screen, matchingServices.Select(e => e.ToVM()));
  }

  public void Cancel() {
    if (_serviceNumber == string.Empty) {
      Screen.PopupWindow = null;
      return;
    }
    ServiceNumber = ServiceNumber[..^1];
  }

  public void Overview() {
    Screen.PopupWindow = new SelectConfigPopupVM(Screen, Screen.Ebula.Model.LoadedConfigs.Select(c => c.ToVM()));
  }

  private string _serviceNumber = string.Empty;
  public string ServiceNumber {
    get {
      return _serviceNumber;
    }
    set {
      _serviceNumber = value;
      OnPropertyChanged(nameof(ServiceNumber));
    }
  }
}
