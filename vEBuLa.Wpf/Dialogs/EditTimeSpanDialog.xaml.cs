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
  public static TimeSpan? Time = null;
  public EditTimeSpanDialog(TimeSpan? timeSpan, bool isDeparture, Vector startupLocation) {
    InitializeComponent();

    Header.Text = isDeparture ? "Change Departure" : "Change Arrival";

    Time = timeSpan ?? new TimeSpan();
    txtHour.Text = Time?.Hours.ToString();
    txtMinute.Text = Time?.Minutes.ToString();
    txtSecond.Text = Time?.Seconds.ToString();

    Left = startupLocation.X;
    Top = startupLocation.Y;

    txtHour.SelectAll();
  }

  private void Window_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Enter && e.Key != Key.Escape) return;

    var hour = txtHour.Text == "" ? 0 : int.Parse(txtHour.Text);
    var minute = txtMinute.Text == "" ? 0 : int.Parse(txtMinute.Text);
    var second = txtSecond.Text == "" ? 0 : int.Parse(txtSecond.Text);

    Time = hour > 0 || minute > 0 || second > 0 ? new TimeSpan(hour, minute, second) : null;

    DialogResult = e.Key == Key.Enter;
  }

  private void Focus(object sender, RoutedEventArgs e) {
    if (sender is TextBox box) box.SelectAll();
  }
}
