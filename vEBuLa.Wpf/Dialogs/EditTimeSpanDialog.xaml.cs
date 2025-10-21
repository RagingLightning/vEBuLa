using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using vEBuLa.Models;

namespace vEBuLa.Dialogs;
/// <summary>
/// Interaktionslogik für EditSpeedDialog.xaml
/// </summary>
public partial class EditTimeSpanDialog : Window {
  private ILogger<EditDateTimeDialog>? Logger => App.GetService<ILogger<EditDateTimeDialog>>();
  public static TimeSpan? Time { get; private set; } = null;
  public EditTimeSpanDialog(TimeSpan? timeSpan, Vector startupLocation) {
    InitializeComponent();

    Time = timeSpan;
    if (timeSpan is TimeSpan ts) {
      txtHour.Text = (ts.Hours + 24 * ts.Days).ToString() ?? string.Empty;
      txtMinute.Text = ts.Minutes.ToString() ?? string.Empty;
      txtSecond.Text = ts.Seconds.ToString() ?? string.Empty;
    }

    Left = startupLocation.X;
    Top = startupLocation.Y;

    txtHour.SelectAll();
    Logger?.LogDebug("New Dialog created");
  }

  private void Window_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Enter && e.Key != Key.Escape) return;
    try {
      var hour = txtHour.Text == string.Empty ? -1 : int.Parse(txtHour.Text);
      var minute = txtMinute.Text == string.Empty ? -1 : int.Parse(txtMinute.Text);
      var second = txtSecond.Text == string.Empty ? -1 : int.Parse(txtSecond.Text);

      Time = hour > -1 && minute > -1 && second > -1 ? new TimeSpan(hour, minute, second) : null;
      Logger?.LogDebug("Dialog dismissed, success: {DialogSuccess}", e.Key == Key.Enter);

      DialogResult = e.Key == Key.Enter;
    }
    catch (Exception ex) {
      Logger?.LogWarning(ex, "Exception during Dialog submission");
      DialogResult = false;
    }
  }

  private void Focus(object sender, RoutedEventArgs e) {
    if (sender is TextBox box) box.SelectAll();
  }
}
