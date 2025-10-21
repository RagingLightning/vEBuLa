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
using vEBuLa.ViewModels;

namespace vEBuLa.Views;
/// <summary>
/// Interaktionslogik für StorageConfigScreenV.xaml
/// </summary>
public partial class SetupScreenV : UserControl {
  public SetupScreenV() {
    InitializeComponent();
  }

  private void Service_Click(object sender, RoutedEventArgs e) {
    if (DataContext is not SetupScreenVM vm) return;
    if (sender is not Button b || b.DataContext is not EbulaServiceVM service) return;
    vm.SelectedService = service;
  }

  private void Route_Click(object sender, RoutedEventArgs e) {
    if (DataContext is not SetupScreenVM vm) return;
    if (sender is not Button b || b.DataContext is not EbulaRouteVM route) return;
    vm.SelectedRoute = route;
  }
}
