using System;

namespace vEBuLa.Extensions;
internal static class StringExtensions {

  public static string Crop(this string input, int length) => input.Length <= length ? input : $"{input[..(length-2)]}..";

  public static string BiCrop(this string input, int a, int b) => input.Length <= a+b+2 ? input : string.Concat(input.AsSpan(0,a), "..", input.AsSpan(input.Length-b));

}
