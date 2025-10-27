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
  public string Text { get; private set; } = string.Empty;

  public SimpleTextBoxPopup(string text, Point p, PopupOptions? options = null) : this(text, new Vector(p.X, p.Y), options) { }

  public SimpleTextBoxPopup(string text, Vector startupLocation, PopupOptions? options = null) {
    InitializeComponent();
    options ??= new PopupOptions();
    Text = text;
    txt.Text = text;

    Left = startupLocation.X;
    Top = startupLocation.Y;

    txt.AcceptsReturn = options.Multiline;
    txt.Style = options.Dark ? FindResource("Dark") as Style : null;
    ResizeMode = options.CanResize ? ResizeMode.CanResizeWithGrip : ResizeMode.NoResize;
    Width = options.Width;
    Height = options.Height;
    txt.FontSize = options.FontSize;

    txt.SelectAll();
  }

  private void Window_KeyDown(object sender, KeyEventArgs e) {
    if (txt.AcceptsReturn && Keyboard.IsKeyUp(Key.LeftShift) && Keyboard.IsKeyUp(Key.LeftCtrl)) return;
    if (e.Key != Key.Enter && e.Key != Key.Escape) return;
    if (e.Key == Key.Enter)
      Text = txt.Text;
    DialogResult = e.Key == Key.Enter;
  }
}

public class PopupOptions {
  public bool Multiline { get; set; } = false;
  public bool Dark { get; set; } = false;
  public bool CanResize { get; set; } = true;
  public int Width { get; set; } = 150;
  public int Height { get; set; } = 100;
  public int FontSize { get; set; } = 12;
  public FontWeight FontWeight { get; set; } = FontWeights.Normal;
}