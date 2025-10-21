using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.Models;

namespace vEBuLa.Extensions;
internal static class MathEx {
  public static double Distance(Position a, Position b) => Math.Sqrt(Math.Pow(a.Lat - b.Lat, 2) + Math.Pow(a.Lng - b.Lng, 2));
}
