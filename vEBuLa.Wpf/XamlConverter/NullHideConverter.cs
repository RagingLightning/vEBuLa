using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace vEBuLa.Wpf.XamlConverter;
internal class NullHideConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if (!targetType.IsAssignableFrom(typeof(Visibility))) throw new Exception();

    return value is null ? Visibility.Hidden : Visibility.Visible;

  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    return false;
  }
}
