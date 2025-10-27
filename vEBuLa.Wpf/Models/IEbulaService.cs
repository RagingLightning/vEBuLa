using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.Commands;
using vEBuLa.ViewModels;

namespace vEBuLa.Models;

public interface IEbulaService {

  public EbulaServiceVM ToVM(BaseC? editCommand = null, SetupScreenVM? screen = null);

  public List<EbulaServiceStop> Stops { get; }
  public TimeSpan StartTime { get; set; }
  public string Description { get; set; }
  public List<string> Vehicles { get; set; }
  public string Name { get; set; }
  public List<EbulaSegment> Segments { get; }

}
