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

  public static DateTime ExtrapolateFromTimeFrame(Vector2 posA, DateTime timeA, Vector2 posB, DateTime timeB, Vector2 posC) {
    var t = LinePointPassingScore(posA, posB, posC);
    var timeDiff = timeB - timeA;
    var extrapolatedTime = timeA + (timeDiff * t);
    return extrapolatedTime;
  }

  public static DateTime ExtrapolateFromTimeFrame(int posA, DateTime timeA, int posB, DateTime timeB, int posC) {
    var t = (posC - posA) / (posB - posA);
    var timeDiff = timeB - timeA;
    var extrapolatedTime = timeA + (timeDiff * t);
    return extrapolatedTime;
  }
}
