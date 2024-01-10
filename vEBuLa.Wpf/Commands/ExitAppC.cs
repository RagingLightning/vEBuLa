using System.Windows;

namespace vEBuLa.Commands;
internal class ExitAppC : BaseC {
  public override void Execute(object? parameter) {
    Application.Current.Shutdown();
  }
}
