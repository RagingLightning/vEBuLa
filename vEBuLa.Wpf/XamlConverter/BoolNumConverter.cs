using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace vEBuLa.XamlConverter;
internal class BoolNumConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if (!targetType.IsAssignableFrom(typeof(int))) throw new Exception();
    if (value is not bool) return 0;
    if (parameter is not string param) return 0;
    if (!int.TryParse(param.Split('|')[0], out var a)) return 0;
    if (!int.TryParse(param.Split('|')[1], out var b)) return 0;

    return (bool) value ? a : b;

  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    if (!targetType.IsAssignableFrom(typeof(bool))) throw new Exception();
    return false;
  }
}
