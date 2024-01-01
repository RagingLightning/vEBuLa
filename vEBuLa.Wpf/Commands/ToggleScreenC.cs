using vEBuLa.ViewModels;

namespace vEBuLa.Commands;

class ToggleScreenC : BaseC {
  private readonly EbulaVM ebulaVm;

  public ToggleScreenC(EbulaVM ebulaVm) {
    this.ebulaVm = ebulaVm;
  }

  public override void Execute(object? parameter) {
    ebulaVm.Active = !ebulaVm.Active;
  }
}
