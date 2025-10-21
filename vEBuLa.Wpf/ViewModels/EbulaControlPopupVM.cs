using vEBuLa.Commands.Navigation;
using vEBuLa.Models;

namespace vEBuLa.ViewModels;
class EbulaControlPopupVM : BaseVM {
	private EbulaScreenVM Screen;
  public EbulaControlPopupVM(EbulaScreenVM screen) {
		Screen = screen;
		ScrollBackwards = Screen.ScrollBackwards;
		_delay = Screen.ServiceDelay;
		ScrollMode = Screen.ScrollMode;

		Screen.Ebula.NavigateCommand = new NavigateControlPopupC(this, Screen);

		DirectionFocus = true;
  }

	internal void Close(bool submit) {
		if (submit) {
			Screen.ScrollBackwards = ScrollBackwards;
			Screen.ScrollMode = ScrollMode;
			Screen.FindNextTarget();
			Screen.ServiceDelay = _delay;
		}
		Screen.PopupWindow = null;
	}

  internal void NextGroup() {
		if (DirectionFocus) {
			DirectionFocus = false;
			DelayFocus = true;
			return;
		}

		if (DelayFocus) {
			DelayFocus = false;
			ScrollModeFocus = true;
			return;
		}

		ScrollModeFocus = false;
		DirectionFocus = true;
  }

  internal void NextSetting() {
		if (DelayFocus)
			return;

		if (DirectionFocus) {
			ScrollBackwards = ScrollForwards;
			return;
		}

		ScrollMode = ScrollMode switch {
			EbulaScrollMode.POSITION => EbulaScrollMode.TIME,
			EbulaScrollMode.TIME => EbulaScrollMode.MANUAL,
			EbulaScrollMode.MANUAL => EbulaScrollMode.POSITION,
			_ => EbulaScrollMode.POSITION
		};
  }

  internal void PrevSetting() {
    if (DelayFocus)
			return;

		if (DirectionFocus) {
			ScrollBackwards = ScrollForwards;
			return;
		}

    ScrollMode = ScrollMode switch {
      EbulaScrollMode.MANUAL => EbulaScrollMode.TIME,
      EbulaScrollMode.TIME => EbulaScrollMode.POSITION,
      EbulaScrollMode.POSITION => EbulaScrollMode.MANUAL,
      _ => EbulaScrollMode.MANUAL
    };
  }

	internal void AppendDigit(int digit) {
		if (!DelayFocus) return;

		if (InitialDelayFocus) {
			_delay = digit;
			InitialDelayFocus = false;
      OnPropertyChanged(nameof(Delay));
      return;
		}

		_delay *= 10;
		_delay += digit;
		OnPropertyChanged(nameof(Delay));
	}

	#region Properties

	private bool _scrollBackwards;
	public bool ScrollForwards => !_scrollBackwards;
	public bool ScrollBackwards {
		get {
			return _scrollBackwards;
		}
		set {
			_scrollBackwards = value;
			OnPropertyChanged(nameof(ScrollBackwards));
			OnPropertyChanged(nameof(ScrollForwards));
      OnPropertyChanged(nameof(ScrollUpFocus));
      OnPropertyChanged(nameof(ScrollDownFocus));
    }
	}

	private int _delay;
	public string Delay {
		get {
			return _delay.ToString();
		}
		set {
			if (int.TryParse(value, out int delay))
				_delay = delay;
			OnPropertyChanged(nameof(Delay));
		}
  }

  private EbulaScrollMode _scrollMode;
  public EbulaScrollMode ScrollMode {
    get {
      return _scrollMode;
    }
    set {
      _scrollMode = value;
      OnPropertyChanged(nameof(PositionScrolling));
      OnPropertyChanged(nameof(TimeScrolling));
      OnPropertyChanged(nameof(ManualScrolling));
      OnPropertyChanged(nameof(ScrollPositionFocus));
      OnPropertyChanged(nameof(ScrollTimeFocus));
      OnPropertyChanged(nameof(ScrollManualFocus));
    }
  }

	public bool PositionScrolling {
		get {
			return ScrollMode == EbulaScrollMode.POSITION;
		}
		set {
			if (value)
				ScrollMode = EbulaScrollMode.POSITION;
		}
  }

	public bool TimeScrolling {
		get {
			return ScrollMode == EbulaScrollMode.TIME;
    }
		set {
			if (value)
				ScrollMode = EbulaScrollMode.TIME;
		}
  }

	public bool ManualScrolling {
		get {
			return ScrollMode == EbulaScrollMode.MANUAL;
    }
		set {
			if (value)
				ScrollMode = EbulaScrollMode.MANUAL;
    }
  }

  #region Focus Control
  private bool _directionFocus;
	public bool DirectionFocus {
		get {
			return _directionFocus;
		}
		set {
			_directionFocus = value;
			OnPropertyChanged(nameof(DirectionFocus));
			OnPropertyChanged(nameof(ScrollUpFocus));
      OnPropertyChanged(nameof(ScrollDownFocus));
    }
	}

	public bool ScrollUpFocus => DirectionFocus && ScrollForwards;
  public bool ScrollDownFocus => DirectionFocus && ScrollBackwards;

	private bool _delayFocus;
	public bool DelayFocus {
		get {
			return _delayFocus;
		}
		set {
			_delayFocus = value;
			InitialDelayFocus = true;
			OnPropertyChanged(nameof(DelayFocus));
		}
	}

	private bool InitialDelayFocus { get; set; }

	private bool _scrollModeFocus;
	public bool ScrollModeFocus {
		get {
			return _scrollModeFocus;
		}
		set {
			_scrollModeFocus = value;
			OnPropertyChanged(nameof(ScrollModeFocus));
      OnPropertyChanged(nameof(ScrollPositionFocus));
      OnPropertyChanged(nameof(ScrollTimeFocus));
      OnPropertyChanged(nameof(ScrollManualFocus));
    }
	}

  public bool ScrollPositionFocus => ScrollModeFocus && PositionScrolling;
  public bool ScrollTimeFocus => ScrollModeFocus && TimeScrolling;
	public bool ScrollManualFocus => ScrollModeFocus && ManualScrolling;

  #endregion

  #endregion
}
