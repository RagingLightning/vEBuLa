using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using vEBuLa.ViewModels;

namespace vEBuLa.Views;
/// <summary>
/// Interaktionslogik für EbulaView.xaml
/// </summary>
public partial class EbulaScreenV : UserControl {
  public EbulaScreenV() {
    InitializeComponent();
  }

  private void ListView_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
    if (sender is not ListView lv) return;
    if (lv.DataContext is not EbulaScreenVM vm) return;
    e.Handled = vm.Ebula.NormalMode;
  }

  private void ListView_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
    if (sender is not ListView lv) return;
    if (lv.DataContext is not EbulaScreenVM vm) return;
    e.Handled = vm.Ebula.NormalMode;

  }
}
