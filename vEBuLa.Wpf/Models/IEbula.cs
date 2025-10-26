using System.Collections.Generic;

namespace vEBuLa.Models;
public interface IEbula {
  EbulaConfig? Config { get; }
  List<EbulaConfig> LoadedConfigs { get; }
}