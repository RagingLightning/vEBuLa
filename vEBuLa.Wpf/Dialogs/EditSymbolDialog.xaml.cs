using System.Windows;
using System.Windows.Input;
using vEBuLa.Models;

namespace vEBuLa.Dialogs;
/// <summary>
/// Interaktionslogik für EditSpeedDialog.xaml
/// </summary>
public partial class EditSymbolDialog : Window {
  public static EbulaSymbol Symbol;
  public EditSymbolDialog(EbulaSymbol ebulaSymbol, Vector startupLocation) {
    InitializeComponent();
    Symbol = ebulaSymbol;

    if (Symbol == EbulaSymbol.ZUGFUNK) btnFunk.IsEnabled = false;
    else if (Symbol == EbulaSymbol.BERMSWEG_KURZ) btnBremsweg.IsEnabled = false;
    else if (Symbol == EbulaSymbol.WEICHENBEREICH) btnWeiche.IsEnabled = false;
    else if (Symbol == EbulaSymbol.STUMPFGLEIS) btnBremsweg.IsEnabled = false;
    else btnNone.IsEnabled = false;

    Left = startupLocation.X;
    Top = startupLocation.Y;
  }

  private void Window_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Escape) return;
    DialogResult = false;
  }

  private void Select_None(object sender, RoutedEventArgs e) {
    Symbol = EbulaSymbol.NONE;
    DialogResult = true;
  }

  private void Select_Weichen(object sender, RoutedEventArgs e) {
    Symbol = EbulaSymbol.WEICHENBEREICH;
    DialogResult = true;
  }

  private void Select_Stumpf(object sender, RoutedEventArgs e) {
    Symbol = EbulaSymbol.STUMPFGLEIS;
    DialogResult = true;
  }

  private void Select_Bremsweg(object sender, RoutedEventArgs e) {
    Symbol = EbulaSymbol.BERMSWEG_KURZ;
    DialogResult = true;
  }

  private void Select_Zugfunk(object sender, RoutedEventArgs e) {
    Symbol = EbulaSymbol.ZUGFUNK;
    DialogResult = true;
  }
}
