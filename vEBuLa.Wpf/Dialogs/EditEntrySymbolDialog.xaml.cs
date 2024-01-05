using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Input;
using vEBuLa.Models;

namespace vEBuLa.Dialogs;
/// <summary>
/// Interaktionslogik für EditSpeedDialog.xaml
/// </summary>
public partial class EditEntrySymbolDialog : Window {
  private ILogger<EditEntrySymbolDialog>? Logger => App.GetService<ILogger<EditEntrySymbolDialog>>();
  public static EbulaSymbol Symbol { get; private set; } = EbulaSymbol.NONE;
  public EditEntrySymbolDialog(EbulaSymbol ebulaSymbol, Vector startupLocation) {
    InitializeComponent();
    Symbol = ebulaSymbol;

    if (Symbol == EbulaSymbol.ZUGFUNK) btnFunk.IsEnabled = false;
    else if (Symbol == EbulaSymbol.BERMSWEG_KURZ) btnBremsweg.IsEnabled = false;
    else if (Symbol == EbulaSymbol.WEICHENBEREICH) btnWeiche.IsEnabled = false;
    else if (Symbol == EbulaSymbol.STUMPFGLEIS) btnBremsweg.IsEnabled = false;
    else btnNone.IsEnabled = false;

    Left = startupLocation.X;
    Top = startupLocation.Y;
    Logger?.LogDebug("New Dialog created");
  }

  private void Window_KeyDown(object sender, KeyEventArgs e) {
    if (e.Key != Key.Escape) return;
    Logger?.LogDebug("Dialog dismissed, success: {DialogSuccess}", false);
    DialogResult = false;
  }

  private void Select_None(object sender, RoutedEventArgs e) {
    Symbol = EbulaSymbol.NONE;
    Logger?.LogDebug("Dialog dismissed, success: {DialogSuccess}", true);
    DialogResult = true;
  }

  private void Select_Weichen(object sender, RoutedEventArgs e) {
    Symbol = EbulaSymbol.WEICHENBEREICH;
    Logger?.LogDebug("Dialog dismissed, success: {DialogSuccess}", true);
    DialogResult = true;
  }

  private void Select_Stumpf(object sender, RoutedEventArgs e) {
    Symbol = EbulaSymbol.STUMPFGLEIS;
    Logger?.LogDebug("Dialog dismissed, success: {DialogSuccess}", true);
    DialogResult = true;
  }

  private void Select_Bremsweg(object sender, RoutedEventArgs e) {
    Symbol = EbulaSymbol.BERMSWEG_KURZ;
    Logger?.LogDebug("Dialog dismissed, success: {DialogSuccess}", true);
    DialogResult = true;
  }

  private void Select_Zugfunk(object sender, RoutedEventArgs e) {
    Symbol = EbulaSymbol.ZUGFUNK;
    Logger?.LogDebug("Dialog dismissed, success: {DialogSuccess}", true);
    DialogResult = true;
  }
}
