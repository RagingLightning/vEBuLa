using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.Commands;
using vEBuLa.ViewModels;

namespace vEBuLa.Models;
class EbulaCustomService : IEbulaService {
  public List<EbulaServiceStop> Stops { get; } = new();
  public string Description { get; set; } = string.Empty;
  public List<string> Vehicles { get; set; } = new();
  public TimeSpan StartTime { get; set; }
  public string Name { get; set; }
  public List<EbulaSegment> Segments { get; }

  public EbulaCustomService(IEnumerable<EbulaSegment> segments, TimeSpan startTime, string name) {
    Segments = segments.ToList();
    StartTime = startTime;
    Name = name;
  }

  public EbulaServiceVM ToVM(BaseC? editCommand = null, SetupScreenVM screen = null) {
    return new EbulaServiceVM(this, editCommand);
  }
}
