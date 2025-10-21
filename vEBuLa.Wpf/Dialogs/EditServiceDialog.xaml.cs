using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using vEBuLa.Models;

namespace vEBuLa.Dialogs;
/// <summary>
/// Interaktionslogik für EditServiceDialog.xaml
/// </summary>
public partial class EditServiceDialog : Window {
  private ILogger<EditServiceDialog>? Logger => App.GetService<ILogger<EditServiceDialog>>();
  public static string ServiceName { get; private set; } = string.Empty;
  public static string Description { get; private set; } = string.Empty;
  public static TimeSpan StartTime { get; private set; } = TimeSpan.Zero;
  public static string Vehicle { get; private set; } = string.Empty;
  public static bool Delete { get; private set; } = false;

  public EditServiceDialog(TimeSpan startTime, string vehicle, Vector startupLocation) : this(string.Empty, string.Empty, startTime, vehicle, startupLocation) { }
  public EditServiceDialog(string name, string description, TimeSpan startTime, string vehicle, Vector startupLocation) {
    InitializeComponent();

    ServiceName = name;
    Description = description;
    StartTime = startTime;
    Vehicle = vehicle;
    Delete = false;

    txtMain.Text = name;
    txtSecond.Text = description;
    txtTimeHr.Text = startTime.Hours.ToString();
    txtTimeMn.Text = startTime.Minutes.ToString();
    txtTimeSc.Text = startTime.Seconds.ToString();
    txtVehicle.Text = vehicle;

    Left = startupLocation.X;
    Top = startupLocation.Y;
    
    txtMain.SelectAll();
    Logger?.LogDebug("New Dialog created");
  }

  private void Window_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Enter && e.Key != Key.Escape) return;
    try {
      ServiceName = txtMain.Text;
      Description = txtSecond.Text;
      var hr = txtTimeHr.Text == string.Empty ? 0 : int.TryParse(txtTimeHr.Text, out var h) ? h : 0;
      var mn = txtTimeHr.Text == string.Empty ? 0 : int.TryParse(txtTimeMn.Text, out var m) ? m : 0;
      var sc = txtTimeHr.Text == string.Empty ? 0 : int.TryParse(txtTimeSc.Text, out var s) ? s : 0;
      StartTime = TimeSpan.Zero + TimeSpan.FromHours(hr) + TimeSpan.FromMinutes(mn) + TimeSpan.FromSeconds(sc);
      Vehicle = txtVehicle.Text;
      Logger?.LogDebug("Dialog dismissed, success: {DialogSuccess}", e.Key == Key.Enter);
      DialogResult = e.Key == Key.Enter;
    } catch (Exception ex) {
      Logger?.LogWarning(ex, "Exception during Dialog submission");
      DialogResult = false;
    }
  }

  private void Delete_Click(object sender, RoutedEventArgs e) {
    Delete = true;
    DialogResult = true;
  }

  private void Focus(object sender, RoutedEventArgs e) {
    if (sender is TextBox box) box.SelectAll();
  }
}
