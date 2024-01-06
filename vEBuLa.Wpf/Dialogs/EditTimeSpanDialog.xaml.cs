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

    Time = timeSpan;
    txtHour.Text = Time?.Hours.ToString() ?? string.Empty;
    txtMinute.Text = Time?.Minutes.ToString() ?? string.Empty;
    txtSecond.Text = Time?.Seconds.ToString() ?? string.Empty;

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
