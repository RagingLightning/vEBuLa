using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Models;

namespace vEBuLa.Dialogs;
/// <summary>
/// Interaktionslogik für EditSpeedDialog.xaml
/// </summary>
public partial class EditEntryNameDialog : Window {
  private ILogger<EditEntryNameDialog>? Logger => App.GetService<ILogger<EditEntryNameDialog>>();
  public static string EntryName { get; private set; } = string.Empty;
  public static bool NameBold { get; private set; } = false;
  public static string Description { get; private set; } = string.Empty;
  public static bool DescriptionBold { get; private set; } = false;
  public EditEntryNameDialog(string name, string description, bool nameBold, bool descBold, Vector startupLocation) {
    InitializeComponent();

    EntryName = name;
    Description = description;
    NameBold = nameBold;
    DescriptionBold = descBold;

    txtMain.Text = name;
    txtSecond.Text = description;
    cbxMain.IsChecked = nameBold;
    cbxSecond.IsChecked = descBold;

    Left = startupLocation.X;
    Top = startupLocation.Y;
    
    txtMain.SelectAll();
    Logger?.LogDebug("New Dialog created");
  }

  private void Window_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Enter && e.Key != Key.Escape) return;
    try {
      EntryName = txtMain.Text;
      Description = txtSecond.Text;
      NameBold = cbxMain.IsChecked == true;
      DescriptionBold = cbxSecond.IsChecked == true;
      Logger?.LogDebug("Dialog dismissed, success: {DialogSuccess}", e.Key == Key.Enter);
      DialogResult = e.Key == Key.Enter;
    } catch (Exception ex) {
      Logger?.LogWarning(ex, "Exception during Dialog submission");
      DialogResult = false;
    }
  }
}
