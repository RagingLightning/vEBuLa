using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace vEBuLa.XamlConverter;
internal class NullHideConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if (!targetType.IsAssignableFrom(typeof(Visibility))) throw new Exception();
    bool result = false;
    if (value is null) result = true;
    if (value is string s) result = s == string.Empty;

    return parameter is null ? result ? Visibility.Hidden : Visibility.Visible : result ? Visibility.Visible : Visibility.Hidden;

  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    return false;
  }
}

internal class NullVisConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if (!targetType.IsAssignableFrom(typeof(Visibility))) throw new Exception();
    bool result = false;
    if (value is null) result = true;
    if (value is string s) result = s == string.Empty;

    return parameter is null ? result ? Visibility.Collapsed : Visibility.Visible : result ? Visibility.Visible : Visibility.Collapsed;

  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    return false;
  }
}
