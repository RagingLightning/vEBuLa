using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace vEBuLa.Models;

public class ProgramConfig {
  public ProgramConfig(string path) {
    if (!File.Exists(path))
      return;

    foreach (var line in File.ReadAllLines(path)) {
      if (string.IsNullOrWhiteSpace(line))
        continue;

      if (line.StartsWith("//"))
        continue;

      var keyValue = line.Split('=', 2).Select(e => e.Trim()).ToArray();

      if (keyValue.Length != 2)
        continue;

      switch (keyValue[0]) {
        case "GlobalHotkeys":
          if (bool.TryParse(keyValue[1], out var gh))
            GlobalHotkeys = gh;
          break;
        case "ApiKey_TSW6":
          ApiKey_TSW6 = keyValue[1];
          break;
      }
    }
  }

  internal ProgramConfig() { }

  public bool GlobalHotkeys { get; private set; } = false;

  public string ApiKey_TSW6 { get; private set; } = string.Empty;

}
