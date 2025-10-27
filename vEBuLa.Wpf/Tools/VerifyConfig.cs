using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.Models;

namespace vEBuLa.Tools;

internal class VerifyConfig {

  public static void Run(FileInfo configFile) {
    Log.Information("+---------------------");
    Log.Information("| vEBuLa v0.1 - Config verification tool");
    Log.Information("| EBuLa config file: {file}", configFile);
    Log.Information("+---------------------");
    Log.Information("");

    if (!configFile.Exists) {
      Log.Error("CSV data file {file} does not exist, exiting", configFile);
      return;
    }

    var config = new EbulaConfig(configFile.FullName);
  }
}
