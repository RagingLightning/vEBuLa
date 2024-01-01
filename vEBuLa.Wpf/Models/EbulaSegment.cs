using System.Collections.Generic;

namespace vEBuLa.Models;
internal class EbulaSegment {
  public string Origin { get; set; }
  public string Destination { get; set; }
  public List<EbulaEntry> Entries { get; set; }
}
