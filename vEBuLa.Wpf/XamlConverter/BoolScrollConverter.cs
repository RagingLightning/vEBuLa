using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace vEBuLa.XamlConverter;
internal class BoolScrollConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if (!targetType.IsAssignableFrom(typeof(ScrollBarVisibility))) throw new Exception();
    if (value is not bool) return ScrollBarVisibility.Hidden;

    return (bool) value ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Disabled;

  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    if (!targetType.IsAssignableFrom(typeof(bool))) throw new Exception();
    if (value is not ScrollBarVisibility) return false;

    return (ScrollBarVisibility) value == ScrollBarVisibility.Hidden;
  }
}
