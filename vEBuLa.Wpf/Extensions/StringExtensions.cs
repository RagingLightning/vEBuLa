using System;

namespace vEBuLa.Extensions;
internal static class StringExtensions {

  public static string Crop(this string input, int length) => input.Length < length ? input : $"{input[..length]}..";

  public static string BiCrop(this string input, int a, int b) => input.Length < a+b ? input : string.Concat(input.AsSpan(0,a), "..", input.AsSpan(input.Length-b));

}
