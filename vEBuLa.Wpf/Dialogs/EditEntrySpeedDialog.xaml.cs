using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using System.Windows.Input;

namespace vEBuLa.Dialogs;
/// <summary>
/// Interaktionslogik für EditSpeedDialog.xaml
/// </summary>
public partial class EditEntrySpeedDialog : Window {
  private ILogger<EditEntrySpeedDialog>? Logger => App.GetService<ILogger<EditEntrySpeedDialog>>();
  public static int Speed { get; private set; } = 0;
  public static bool Signed { get; private set; } = true;
  public EditEntrySpeedDialog(int speed, bool signed, Vector startupLocation) {
    InitializeComponent();
    txtSpeed.Text = speed.ToString();
    cbxSigned.IsChecked = signed;
    Left = startupLocation.X;
    Top = startupLocation.Y;

    txtSpeed.SelectAll();
    Logger?.LogDebug("New Dialog created");
  }

  private void Window_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Enter && e.Key != Key.Escape) return;
    try {
      Speed = txtSpeed.Text == string.Empty ? 0 : int.Parse(txtSpeed.Text);
      Signed = cbxSigned.IsChecked == true;
      Logger?.LogDebug("Dialog dismissed, success: {DialogSuccess}", e.Key == Key.Enter);
      DialogResult = e.Key == Key.Enter;
    } catch (Exception ex) {
      Logger?.LogWarning(ex, "Exception during Dialog submission");
      DialogResult = false;
    }
  }
}
