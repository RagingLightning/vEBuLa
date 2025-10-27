using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vEBuLa.Models;

namespace vEBuLa.Tools.Export;

internal class StopsToCsv {

  public static void Run(FileInfo input, FileInfo output, string serviceNumber) {
    Log.Information("+---------------------");
    Log.Information("| vEBuLa v0.1 - Service stops to csv exporter");
    Log.Information("| EBuLa config: {input}", input);
    Log.Information("| Output      : {output}", output);
    Log.Information("+---------------------");
    Log.Information("");

    if (!input.Exists) {
      Log.Error("Input file {file} does not exist, exiting", input);
      return;
    }

    var preset = new EbulaConfig(input.FullName);
    var service = preset.Services.Values.FirstOrDefault(s => s.Number == serviceNumber);

    if (service is null) {
      Log.Error("Service #{serviceNr} does not exist, exiting", serviceNumber);
      return;
    }

    if (service.Stops.Count == 0) {
      Log.Warning("Service {service} has no stops, exiting", service.Description);
      return;
    }

    Log.Information("Service {service} has {stopCount} stops", service.Description, service.Stops.Count);

    var sb = new StringBuilder("Location;Name;Arrival;Departure;Bold\r\n");
    foreach (var stop in service.Stops) {
      Log.Information("Processing {stop}...", stop);

      var name = stop.EntryName.Replace('\n', '|');
      var arrival = stop.Arrival?.ToString("HH':'mm':'ss") ?? "";
      var departure = stop.Departure?.ToString("HH':'mm':'ss") ?? "";
      sb.AppendLine($"{stop.EntryLocation:D6};{name};{arrival};{departure};{stop.Bold}");
    }

    Log.Information("Writing csv data to {file}", output);

    File.WriteAllText(output.FullName, sb.ToString(), Encoding.UTF8);

    Log.Information("Export complete!");
  }
}
