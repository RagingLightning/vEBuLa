using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using vEBuLa.Models;

namespace vEBuLa.Dialogs;
/// <summary>
/// Interaktionslogik für EditSpeedDialog.xaml
/// </summary>
public partial class EditStationDialog : Window {
  private ILogger<EditStationDialog>? Logger => App.GetService<ILogger<EditStationDialog>>();
  public static string StationName { get; private set; } = string.Empty;
  public EditStationDialog(string stationName, Vector startupLocation) {
    InitializeComponent();

    StationName = stationName;
    txtName.Text = stationName;
    Left = startupLocation.X;
    Top = startupLocation.Y;

    txtName.SelectAll();
    Logger?.LogDebug("New Dialog created");

  }

  private void Window_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Enter && e.Key != Key.Escape) return;

    StationName = txtName.Text;

    Logger?.LogDebug("Dialog dismissed, success: {DialogSuccess}", e.Key == Key.Enter);
    DialogResult = e.Key == Key.Enter;
  }

  private void Focus(object sender, RoutedEventArgs e) {
    if (sender is TextBox box) box.SelectAll();
  }
}
