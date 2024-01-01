using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.Wpf.ViewModels;

namespace vEBuLa.Wpf.Commands.EbulaButtons;

class ToggleScreenC : BaseC {
  private readonly EbulaVM ebulaVm;

  public ToggleScreenC(EbulaVM ebulaVm) {
    this.ebulaVm = ebulaVm;
  }

  public override void Execute(object? parameter) {
    ebulaVm.Active = !ebulaVm.Active;
  }
}
