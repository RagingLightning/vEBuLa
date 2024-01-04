using System.Windows;
using System.Windows.Input;
using vEBuLa.Models;

namespace vEBuLa.Dialogs;
/// <summary>
/// Interaktionslogik für EditSpeedDialog.xaml
/// </summary>
public partial class EditTunnelDialog : Window {
  public static bool TunnelStart = false;
  public static bool TunnelEnd = false;
  public EditTunnelDialog(bool tunnelStart, bool tunnelEnd, Vector startupLocation) {
    InitializeComponent();
    TunnelStart = tunnelStart;
    TunnelEnd = tunnelEnd;
    if (TunnelStart) btnStart.IsEnabled = false;
    else if (TunnelEnd) btnEnd.IsEnabled = false;
    else btnNone.IsEnabled = false;
  }

  private void Window_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Escape) return;
    DialogResult = false;
  }

  private void Select_None(object sender, RoutedEventArgs e) {
    TunnelStart = false;
    TunnelEnd = false;
    DialogResult = true;
  }

  private void Select_Start(object sender, RoutedEventArgs e) {
    TunnelStart = true;
    TunnelEnd = false;
    DialogResult = true;
  }

  private void Select_End(object sender, RoutedEventArgs e) {
    TunnelStart = false;
    TunnelEnd = true;
    DialogResult = true;
  }
}
