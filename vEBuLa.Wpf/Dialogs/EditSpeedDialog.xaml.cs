using System.Windows;
using System.Windows.Input;

namespace vEBuLa.Dialogs;
/// <summary>
/// Interaktionslogik für EditSpeedDialog.xaml
/// </summary>
public partial class EditSpeedDialog : Window {
  public static int Speed = 0;
  public static bool Signed = true;
  public EditSpeedDialog(int speed, bool signed, Vector startupLocation) {
    InitializeComponent();
    txtSpeed.Text = speed.ToString();
    cbxSigned.IsChecked = signed;
    Left = startupLocation.X;
    Top = startupLocation.Y;

    txtSpeed.SelectAll();
  }

  private void Window_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Enter && e.Key != Key.Escape) return;
    Speed = txtSpeed.Text == string.Empty ? 0 : int.Parse(txtSpeed.Text);
    Signed = cbxSigned.IsChecked == true;
    DialogResult = e.Key == Key.Enter;
  }
}
