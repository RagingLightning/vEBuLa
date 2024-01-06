using System;

namespace vEBuLa.Extensions;
internal static class StringExtensions {

  public static string Crop(this string input, int length) => input.Substring(0, Math.Min(input.Length, length));

  public static string BiCrop(this string input, int a, int b) => input.Length < a+b ? input : input.Substring(0,a) + ".." + input.Substring(input.Length-b);

}
