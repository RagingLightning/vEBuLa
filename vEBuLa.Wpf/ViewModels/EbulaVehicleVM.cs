using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vEBuLa.ViewModels;
internal class EbulaVehicleVM : BaseVM {
  
  public EbulaVehicleVM(string vehicle, int serviceCount) {
    Vehicle = vehicle;
		Services = serviceCount;
  }

	#region Properties
	private string _vehiche;
	public string Vehicle {
		get {
			return _vehiche;
		}
		set {
			_vehiche = value;
			OnPropertyChanged(nameof(Vehicle));
		}
	}

	private int _services;
	public int Services {
		get {
			return _services;
		}
		set {
			_services = value;
			OnPropertyChanged(nameof(Services));
		}
	}

	public string Info => $"{Vehicle} [{Services}]";

	private bool _selected = false;
	public bool Selected {
		get {
			return _selected;
		}
		set {
			_selected = value;
			OnPropertyChanged(nameof(Selected));
		}
	}
	#endregion
}
