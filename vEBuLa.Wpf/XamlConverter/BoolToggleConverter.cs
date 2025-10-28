using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace vEBuLa.XamlConverter;

internal class BoolToggleConverter : IValueConverter {

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if (!targetType.IsAssignableFrom(parameter.GetType())) throw new Exception();
    if (value is not bool) return null!;

    return (bool) value ? parameter : null!;

  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    if (!targetType.IsAssignableFrom(typeof(bool))) throw new Exception();

    return value is not null;
  }
}
