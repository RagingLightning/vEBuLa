using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Models;
using vEBuLa.ViewModels;

namespace vEBuLa.Dialogs;
/// <summary>
/// Interaktionslogik für EditSpeedDialog.xaml
/// </summary>
public partial class EditEntryLocationDialog : Window {
  private ILogger<EditEntryLocationDialog>? Logger => App.GetService<ILogger<EditEntryLocationDialog>>();
  public static int Location { get; private set; } = 0;
  public static Gradient Gradient { get; private set; } = 0;
  public EditEntryLocationDialog(int location, Gradient gradient, Vector startupLocation) {
    InitializeComponent();
    txtLocation.Text = location.ToString();
    if (gradient == Gradient.BELOW_10) rbtGradient0.IsChecked = true;
    else if (gradient == Gradient.BELOW_20) rbtGradient1.IsChecked = true;
    else if (gradient == Gradient.BELOW_30) rbtGradient2.IsChecked = true;
    else rbtGradient3.IsChecked = true;
    Left = startupLocation.X;
    Top = startupLocation.Y;
    
    txtLocation.SelectAll();

    Logger?.LogDebug("New Dialog created");
  }

  private void Window_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Enter && e.Key != Key.Escape) return;
    try {
      Location = txtLocation.Text == string.Empty ? 0 : int.Parse(txtLocation.Text);
      if (rbtGradient0.IsChecked == true) Gradient = Gradient.BELOW_10;
      else if (rbtGradient1.IsChecked == true) Gradient = Gradient.BELOW_20;
      else if (rbtGradient2.IsChecked == true) Gradient = Gradient.BELOW_30;
      else Gradient = Gradient.ABOVE_30;
      Logger?.LogDebug("Dialog dismissed, success: {DialogSuccess}", e.Key == Key.Enter);
      DialogResult = e.Key == Key.Enter;
    } catch (Exception ex) {
      Logger?.LogWarning(ex, "Exception during Dialog submission");
      DialogResult = false;
    }
  }
}
