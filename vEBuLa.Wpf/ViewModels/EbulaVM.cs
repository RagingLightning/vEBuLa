using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.Wpf.Commands;
using vEBuLa.Wpf.Commands.EbulaButtons;

namespace vEBuLa.Wpf.ViewModels;
internal class EbulaVM : BaseVM {

	public EbulaVM(Ebula ebula) {
		Screen = new EbulaScreenVM();

		EbulaEntryVM? ebulaEntry = null;
		foreach (var entry in ebula.Entries) {
			ebulaEntry = new EbulaEntryVM(entry, new TimeSpan(0, 0, 0), ebulaEntry);
      Screen.Entries.Add(ebulaEntry);
		}

    Screen.PropertyChanged += Screen_NavigateCommandChanged;
		Screen.UpdateList();

		ToggleScreenCommand = new ToggleScreenC(this);
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
	private BaseC _toggleScreenCommand;
	public BaseC ToggleScreenCommand {
		get {
			return _toggleScreenCommand;
		}
		set {
			_toggleScreenCommand = value;
			OnPropertyChanged(nameof(ToggleScreenCommand));
		}
	}

	public BaseC NavigateCommand => Screen.NavigateCommand; //managed by the screen instance
	#endregion

	#region Other

	private bool _active = true;
	public bool Active {
		get {
			return _active;
		}
		set {
			_active = value;
			OnPropertyChanged(nameof(Active));
		}
	}


  #endregion

  #endregion

}
