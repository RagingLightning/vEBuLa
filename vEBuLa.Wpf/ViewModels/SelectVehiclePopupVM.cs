using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using vEBuLa.Commands.Navigation;
using vEBuLa.Extensions;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;

internal class SelectVehiclePopupVM : BaseVM {
  EbulaScreenVM Screen;
  public EbulaConfigVM Config { get; }
  public ObservableCollection<EbulaVehicleVM> Vehicles { get; } = new();

  private EbulaVehicleVM _selectedVehicle;
  public EbulaVehicleVM SelectedVehicle {
    get {
      return _selectedVehicle;
    }
    set {
      _selectedVehicle = value;
      OnPropertyChanged(nameof(SelectedVehicle));
    }
  }
  public int SelectedVehicleIndex { get; set; }

  public SelectVehiclePopupVM(EbulaScreenVM screen, EbulaConfigVM config) {
    Screen = screen;
    Config = config;
    Vehicles.AddRange(config.Model.Vehicles.Select(v => new EbulaVehicleVM(v.Key, v.Value)));
    SelectedVehicle = Vehicles[0];
    Vehicles[0].Selected = true;

    Screen.Ebula.NavigateCommand = new NavigateSelectVehiclePopupC(screen, this);
  }

  internal void Close(bool accept) {
    if (accept)
      Screen.PopupWindow = new SelectServicePopupVM(Screen, Config.Model.Services.Where(s => s.Value.Vehicles.Contains(SelectedVehicle.Vehicle)).Select(s => s.Value.ToVM()));
    else
      Screen.PopupWindow = null;
  }

  internal void Previous() {
    SelectedVehicle.Selected = false;
    SelectedVehicle = Vehicles[Math.Max(SelectedVehicleIndex - 1, 0)];
    SelectedVehicle.Selected = true;
  }

  internal void Next() {
    SelectedVehicle.Selected = false;
    SelectedVehicle = Vehicles[Math.Min(SelectedVehicleIndex + 1, Vehicles.Count - 1)];
    SelectedVehicle.Selected = true;
  }
}