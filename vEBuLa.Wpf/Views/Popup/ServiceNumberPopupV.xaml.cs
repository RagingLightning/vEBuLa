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
using System.Windows.Navigation;
using System.Windows.Shapes;
using vEBuLa.Commands;
using vEBuLa.Commands.Navigation;
using vEBuLa.ViewModels;

namespace vEBuLa.Views;
/// <summary>
/// Interaktionslogik für ServiceNumberPopupV.xaml
/// </summary>
public partial class ServiceNumberPopupV : UserControl {
  public ServiceNumberPopupV() {
    InitializeComponent();
  }

  private void OnKeyUp(object sender, KeyEventArgs e) {
    if (e.Key != Key.Return) return;
    if (DataContext is not ServiceNumberPopupVM vm) return;
    if (vm.Screen.Ebula.NavigateCommand is not NavigateServiceNumberPopupC cmd) return;
    cmd.Execute(NavAction.ACCEPT);
  }
}
