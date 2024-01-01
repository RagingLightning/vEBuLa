using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace vEBuLa.Wpf.XamlConverter;
internal class BoolHideConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if (!targetType.IsAssignableFrom(typeof(Visibility))) throw new Exception();
    if (value is not bool) return Visibility.Visible;

    return (bool) value ? Visibility.Visible : Visibility.Hidden;

  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    if (!targetType.IsAssignableFrom(typeof(bool))) throw new Exception();
    if (value is not Visibility) return false;

    return (Visibility) value == Visibility.Visible;
  }
}
