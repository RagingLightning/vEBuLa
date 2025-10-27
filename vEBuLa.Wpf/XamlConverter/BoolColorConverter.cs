using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace vEBuLa.XamlConverter;

internal partial class BoolColorConverter : IValueConverter {

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if (!targetType.IsAssignableFrom(typeof(Brush))) throw new Exception();
    if (value is not bool) return Brushes.Red;
    if (parameter is not string param) return Brushes.Red;

    var match = ColorRegex().Match(param);
    if (!match.Success) return Brushes.Red;

    var ra = System.Convert.ToByte(match.Groups["ra"].Value, 16);
    var ga = System.Convert.ToByte(match.Groups["ga"].Value, 16);
    var ba = System.Convert.ToByte(match.Groups["ba"].Value, 16);
    var rb = System.Convert.ToByte(match.Groups["rb"].Value, 16);
    var gb = System.Convert.ToByte(match.Groups["gb"].Value, 16);
    var bb = System.Convert.ToByte(match.Groups["bb"].Value, 16);

    return new SolidColorBrush((bool) value ? Color.FromRgb(ra, ga, ba) : Color.FromRgb(rb, gb, bb));

  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    if (!targetType.IsAssignableFrom(typeof(bool))) throw new Exception();
    return false;
  }

  [GeneratedRegex(@"(?<ra>[0-9a-fA-f]{2})(?<ga>[0-9a-fA-f]{2})(?<ba>[0-9a-fA-f]{2})\|(?<rb>[0-9a-fA-f]{2})(?<gb>[0-9a-fA-f]{2})(?<bb>[0-9a-fA-f]{2})")]
  private static partial Regex ColorRegex();
}