using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace vEBuLa.XamlConverter;
internal partial class BoolNumConverter : IValueConverter {
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

  [GeneratedRegex(@"(?<ra>[0-9a-fA-f]{2});(?<ga>[0-9a-fA-f]{2});(?<ba>[0-9a-fA-f]{2})\|(?<rb>[0-9a-fA-f]{2});(?<gb>[0-9a-fA-f]{2});(?<bb>[0-9a-fA-f]{2})")]
  private static partial Regex ColorRegex();
}