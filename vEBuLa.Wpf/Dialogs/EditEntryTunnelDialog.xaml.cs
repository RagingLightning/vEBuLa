using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Models;

namespace vEBuLa.Dialogs;
/// <summary>
/// Interaktionslogik für EditSpeedDialog.xaml
/// </summary>
public partial class EditEntryTunnelDialog : Window {
  private ILogger<EditEntryTunnelDialog>? Logger => App.GetService<ILogger<EditEntryTunnelDialog>>();
  public static bool TunnelStart { get; private set; } = false;
  public static bool TunnelEnd { get; private set; } = false;
  public EditEntryTunnelDialog(bool tunnelStart, bool tunnelEnd, Vector startupLocation) {
    InitializeComponent();
    Left = startupLocation.X;
    Top = startupLocation.Y;

    TunnelStart = tunnelStart;
    TunnelEnd = tunnelEnd;
    if (TunnelStart) btnStart.IsEnabled = false;
    else if (TunnelEnd) btnEnd.IsEnabled = false;
    else btnNone.IsEnabled = false;
    Logger?.LogDebug("New Dialog created");
  }

  private void Window_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Escape) return;
    Logger?.LogDebug("Dialog dismissed, success: {DialogSuccess}", false);
    DialogResult = false;
  }

  private void Select_None(object sender, RoutedEventArgs e) {
    TunnelStart = false;
    TunnelEnd = false;
    Logger?.LogDebug("Dialog dismissed, success: {DialogSuccess}", true);
    DialogResult = true;
  }

  private void Select_Start(object sender, RoutedEventArgs e) {
    TunnelStart = true;
    TunnelEnd = false;
    Logger?.LogDebug("Dialog dismissed, success: {DialogSuccess}", true);
    DialogResult = true;
  }

  private void Select_End(object sender, RoutedEventArgs e) {
    TunnelStart = false;
    TunnelEnd = true;
    Logger?.LogDebug("Dialog dismissed, success: {DialogSuccess}", true);
    DialogResult = true;
  }
}
