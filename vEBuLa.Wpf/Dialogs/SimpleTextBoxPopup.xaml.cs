using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace vEBuLa.Dialogs;
/// <summary>
/// Interaktionslogik für SimpleTextBoxPopup.xaml
/// </summary>
public partial class SimpleTextBoxPopup : Window {
  public static string Text { get; private set; } = string.Empty;

  private bool Multiline = false;
  public SimpleTextBoxPopup(string text, bool multiline, Vector startupLocation) {
    InitializeComponent();
    txt.Text = text;
    txt.AcceptsReturn = multiline;

    Left = startupLocation.X;
    Top = startupLocation.Y;

    txt.SelectAll();
  }

  private void Window_KeyDown(object sender, KeyEventArgs e) {
    if (Multiline && Keyboard.IsKeyUp(Key.LeftShift) && Keyboard.IsKeyUp(Key.LeftCtrl)) return;
    if (e.Key != Key.Enter && e.Key != Key.Escape) return;
    if (e.Key == Key.Enter)
      Text = txt.Text;
    DialogResult = e.Key == Key.Enter;
  }
}
