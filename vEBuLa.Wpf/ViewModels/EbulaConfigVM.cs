using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;

internal static class EbulaConfigEx { public static EbulaConfigVM ToVM(this EbulaConfig cfg) => new(cfg); }

internal class EbulaConfigVM : BaseVM {
  public EbulaConfig Model { get; }

  public EbulaConfigVM(EbulaConfig model) {
		Model = model;
  }

	#region Properties
	public string Name {
		get {
			return Model.Name;
		}
		set {
			Model.Name = value;
			OnPropertyChanged(nameof(Name));
		}
	}

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
