using Microsoft.Extensions.Logging;
using System;
using System.Numerics;
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
  public static Vector2? GpsLocation { get; private set; } = null;
  public static double? Longitude { get; private set; } = null;
  public static Gradient Gradient { get; private set; } = 0;
  public static bool KilometerBreak { get; private set; } = false;

  private IEbulaGameApi? Api { get; }

  public EditEntryLocationDialog(int location, Vector2? gps,  bool kilometerBreak, Gradient gradient, System.Windows.Vector startupLocation, IEbulaGameApi? api) {
    InitializeComponent();
    Api = api;

    Location = location;
    txtLocation.Text = location.ToString();

    GpsLocation = gps;
    txtLat.Text = gps is null ? "N/A" : gps?.X.ToString("F6");
    txtLng.Text = gps is null ? "N/A" : gps?.Y.ToString("F6");

    cbxBreak.IsChecked = kilometerBreak;
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
      KilometerBreak = cbxBreak.IsChecked == true;
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

  private void SetGpsLocation(object sender, RoutedEventArgs e) {
    if (Api is null || !Api.IsAvailable) return;

    if (Api.GetPosition() is not Vector2 newGps) return;

    GpsLocation = newGps;
    txtLat.Text = newGps.X.ToString("F6");
    txtLng.Text = newGps.Y.ToString("F6");
  }

  private void DelGpsLocation(object sender, RoutedEventArgs e) {
    GpsLocation = null;
    txtLat.Text = "N/A";
    txtLng.Text = "N/A";

  }
}
