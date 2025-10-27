using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using vEBuLa.Models;

namespace vEBuLa.Tools.Import;
internal class StopsFromCsv {

  public static void Run(FileInfo config, FileInfo csvData, string serviceNumber, bool trim) {
    Log.Information("+---------------------");
    Log.Information("| vEBuLa v0.1 - Service stops from csv importer");
    Log.Information("| EBuLa preset : {file}", config);
    Log.Information("| CSV data file: {file}", csvData);
    Log.Information("+---------------------");
    Log.Information("");

    if (!config.Exists) {
      Log.Error("EBuLa preset {file} does not exist, exiting", config);
      return;
    }

    if (!csvData.Exists) {
      Log.Error("CSV data file {file} does not exist, exiting", csvData);
      return;
    }

    if (trim) Log.Warning("Stops not included in CSV file will be removed!");

    var preset = new EbulaConfig(config.FullName);
    var service = preset.Services.Values.FirstOrDefault(s => s.Number == serviceNumber);

    if (service is null) {
      Log.Error("Service #{serviceNr} does not exist, exiting", serviceNumber);
      return;
    }

    var stops = new List<EbulaServiceStop>(service.Stops);
    if (trim) service.Stops.Clear();

    var csvLines = File.ReadLines(csvData.FullName, Encoding.UTF8);
    foreach (var line in csvLines.Skip(1)) {
      var parts = line.Split(';');
      if (parts.Length != 5) {
        Log.Warning("Skipping invalid line: {line}", line);
        continue;
      }

      if (!int.TryParse(parts[0], out var location)) {
        Log.Warning("Skipping line with invalid location: {line}", line);
        continue;
      }

      var name = parts[1].Replace('|', '\n');

      TimeSpan? arrival = null;
      if (TimeSpan.TryParse(parts[2], out var arr))
        arrival = arr;

      TimeSpan? departure = null;
      if (TimeSpan.TryParse(parts[3], out var dep))
        departure = dep;

      var bold = parts[4].Equals("true", StringComparison.OrdinalIgnoreCase);

      var stop = stops.FirstOrDefault(s => s.EntryLocation == location && s.EntryName == name);
      if (stop is null) stop = new EbulaServiceStop(location, name);
      stop.Arrival = arrival is not null ? DateTime.UnixEpoch + arrival : null;
      stop.Departure = departure is not null ? DateTime.UnixEpoch + departure : null;
      stop.Bold = bold;

      Log.Information("Processing {stop}...", stop);

      if (trim) service.Stops.Add(stop);
    }

    Log.Information("Service {service} has {stopCount} stops", service.Description, service.Stops.Count);

    Log.Information("Saving backup file");
    File.Copy(config.FullName, $"{config.FullName}.bak", true);

    Log.Information("Saving updated EBuLa preset");
    preset.Save(config.FullName);

    Log.Information("Import complete!");
  }
}
