using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using vEBuLa.Models;

namespace vEBuLa.Extensions;
internal static class MathEx {

  public static float LinePointPassingScore(Vector2 a, Vector2 b, Vector2 c) {
    var lineAB = b - a;
    var lineAC = c - a;

    var lengthAB = lineAB.LengthSquared();
    if (lengthAB == 0)
      return float.MaxValue;

    var t = Vector2.Dot(lineAC, lineAB) / lineAB.LengthSquared();
    return t;
  }

  public static float SymmtricLinePointPassingScore(Vector2 a, Vector2 b, Vector2 c) {
    return 2 * LinePointPassingScore(a, b, c) - 1f;
  }
}
