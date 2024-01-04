using System.Windows;
using System.Windows.Input;
using vEBuLa.Models;

namespace vEBuLa.Dialogs;
/// <summary>
/// Interaktionslogik für EditSpeedDialog.xaml
/// </summary>
public partial class EditStationDialog : Window {
  public static string Name = string.Empty;
  public static bool NameBold = false;
  public static string Description = string.Empty;
  public static bool DescriptionBold = false;
  public EditStationDialog(string name, string description, bool nameBold, bool descBold, Vector startupLocation) {
    InitializeComponent();

    Name = name;
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
  }

  private void Window_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Enter && e.Key != Key.Escape) return;
    
    Name = txtMain.Text;
    Description = txtSecond.Text;
    NameBold = cbxMain.IsChecked == true;
    DescriptionBold = cbxSecond.IsChecked == true;

    DialogResult = e.Key == Key.Enter;
  }
}
