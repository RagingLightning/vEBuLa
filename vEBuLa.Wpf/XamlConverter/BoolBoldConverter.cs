using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace vEBuLa.Wpf.XamlConverter;
internal class BoolBoldConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if (!targetType.IsAssignableFrom(typeof(FontWeight))) throw new Exception();
    if (value is not bool) return FontWeights.Bold;

    return (bool) value ? FontWeights.Bold : FontWeights.Normal;

  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    if (!targetType.IsAssignableFrom(typeof(bool))) throw new Exception();
    if (value is not FontWeight) return false;

    return (FontWeight) value == FontWeights.Bold;
  }
}
