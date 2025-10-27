using vEBuLa.Commands.Navigation;

namespace vEBuLa.ViewModels;

internal class NoServicePopupVM : BaseVM {
  private EbulaScreenVM Screen;

  public NoServicePopupVM(EbulaScreenVM screen, string serviceNumber) {
    Screen = screen;
    _serviceNumber = serviceNumber;

		Screen.Ebula.NavigateCommand = new NavigateNoServicePopupC(screen);
  }

	private string _serviceNumber;
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