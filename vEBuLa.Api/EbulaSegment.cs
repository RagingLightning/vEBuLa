using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vEBuLa;
internal class EbulaSegment {
  public string Origin { get; set; }
  public string Destination { get; set; }
  public List<EbulaEntry> Entries { get; set; }
}
