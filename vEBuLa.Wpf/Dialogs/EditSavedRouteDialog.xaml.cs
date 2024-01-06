using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using vEBuLa.Models;
using vEBuLa.ViewModels;

namespace vEBuLa.Dialogs;
/// <summary>
/// Interaktionslogik für EditSpeedDialog.xaml
/// </summary>
public partial class EditSavedRouteDialog : Window {
  private ILogger<EditSavedRouteDialog>? Logger => App.GetService<ILogger<EditSavedRouteDialog>>();
  public static string RouteName { get; private set; }
  public static string Description { get; private set; }
  public static int Stations { get; private set; }
  public static TimeSpan Duration { get; private set; }
  public static string Route { get; private set; }

  public EditSavedRouteDialog(Vector startupLocation) : this(string.Empty, string.Empty, 0, TimeSpan.Zero, string.Empty, startupLocation) { }

  public EditSavedRouteDialog(string name, string desc, int stations, TimeSpan duration, string route, Vector startupLocation) {
    InitializeComponent();
    RouteName = name;
    Description = desc;
    Stations = stations;
    Duration = duration;
    Route = route;

    txtName.Text = name;
    txtStations.Text = stations.ToString();
    txtDuration.Text = duration.ToString();
    txtRoute.Text = route;
    
    txtName.SelectAll();

    Top = startupLocation.Y;
    Left = startupLocation.X;

    Logger?.LogDebug("New Dialog created");
  }

  private void Window_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Enter && e.Key != Key.Escape) return;
    try {
      RouteName = txtName.Text;
      Stations = int.Parse(txtStations.Text);
      Duration = TimeSpan.Parse(txtDuration.Text);
      Route = txtRoute.Text;
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

  private void Desc_Click(object sender, RoutedEventArgs e) {
    var dialog = new SimpleTextBoxPopup(Description, true, new Vector(Left + 220, Top - 75));

    if (dialog.ShowDialog() == true)
      Description = SimpleTextBoxPopup.Text;
  }

  private void Save_Click(object sender, RoutedEventArgs e) {
    try {
      RouteName = txtName.Text;
      Stations = int.Parse(txtStations.Text);
      Duration = TimeSpan.Parse(txtDuration.Text);
      Route = txtRoute.Text;
      Logger?.LogDebug("Dialog dismissed, success: {DialogSuccess}", true);
      DialogResult = true;
    }
    catch (Exception ex) {
      Logger?.LogWarning(ex, "Exception during Dialog submission");
      DialogResult = false;
    }

  }
}
