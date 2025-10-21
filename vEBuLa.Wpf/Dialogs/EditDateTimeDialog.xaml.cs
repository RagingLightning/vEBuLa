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
public partial class EditDateTimeDialog : Window {
  private ILogger<EditDateTimeDialog>? Logger => App.GetService<ILogger<EditDateTimeDialog>>();
  public static DateTime? Date { get; private set; } = null;
  public EditDateTimeDialog(DateTime? dateTime, DateTimeDialogType type, Vector startupLocation) {
    InitializeComponent();

    Header.Text = type switch {
      DateTimeDialogType.ARRIVAL => "Change Arrival",
      DateTimeDialogType.DEPARTURE => "Change Departure",
      DateTimeDialogType.DURATION => "Change Duration",
      _ => "Change Time"
    };

    Date = dateTime;
    if (dateTime is DateTime dt) {
      txtHour.Text = (dt.Hour + 24 * (dt.Day - 1)).ToString() ?? string.Empty;
      txtMinute.Text = dt.Minute.ToString() ?? string.Empty;
      txtSecond.Text = dt.Second.ToString() ?? string.Empty;
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

      Date = hour > -1 && minute > -1 && second > -1 ? DateTime.UnixEpoch + new TimeSpan(hour, minute, second) : null;
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

public enum DateTimeDialogType {
  ARRIVAL,
  DEPARTURE,
  DURATION
}
