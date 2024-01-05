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
  private ILogger<EditTimeSpanDialog>? Logger => App.GetService<ILogger<EditTimeSpanDialog>>();
  public static TimeSpan? Time { get; private set; } = null;
  public EditTimeSpanDialog(TimeSpan? timeSpan, TimeSpanDialogType type, Vector startupLocation) {
    InitializeComponent();

    Header.Text = type switch {
      TimeSpanDialogType.ARRIVAL => "Change Arrival",
      TimeSpanDialogType.DEPARTURE => "Change Departure",
      TimeSpanDialogType.DURATION => "Change Duration",
      _ => "Change Time"
    };

    Time = timeSpan ?? new TimeSpan();
    txtHour.Text = Time?.Hours.ToString();
    txtMinute.Text = Time?.Minutes.ToString();
    txtSecond.Text = Time?.Seconds.ToString();

    Left = startupLocation.X;
    Top = startupLocation.Y;

    txtHour.SelectAll();
    Logger?.LogDebug("New Dialog created");
  }

  private void Window_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Enter && e.Key != Key.Escape) return;
    try {
      var hour = txtHour.Text == "" ? 0 : int.Parse(txtHour.Text);
      var minute = txtMinute.Text == "" ? 0 : int.Parse(txtMinute.Text);
      var second = txtSecond.Text == "" ? 0 : int.Parse(txtSecond.Text);

      Time = hour > 0 || minute > 0 || second > 0 ? new TimeSpan(hour, minute, second) : null;
      Logger?.LogDebug("Dialog dismissed, success: {DialogSuccess}", e.Key == Key.Enter);

      DialogResult = e.Key == Key.Enter;
    } catch (Exception ex) {
      Logger?.LogWarning(ex, "Exception during Dialog submission");
      DialogResult = false;
    }
  }

  private void Focus(object sender, RoutedEventArgs e) {
    if (sender is TextBox box) box.SelectAll();
  }
}

public enum TimeSpanDialogType {
  ARRIVAL,
  DEPARTURE,
  DURATION
}
