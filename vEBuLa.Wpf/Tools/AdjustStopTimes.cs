using Microsoft.AspNetCore.Identity.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vEBuLa.Tools;

internal class AdjustStopTimes {

  internal static void Run(FileInfo csvFile, TimeSpan startTime, TimeSpan endTime, TimeSpan targetTime) {
    Log.Information("+---------------------");
    Log.Information("| vEBuLa v0.1 - Stop time adjustment tool");
    Log.Information("| CSV data file  : {file}", csvFile);
    Log.Information("| Start time     : {time}", startTime);
    Log.Information("| End time       : {time}", endTime);
    Log.Information("| Target end time: {time}", targetTime);
    Log.Information("+---------------------");
    Log.Information("");

    if(!csvFile.Exists) {
      Log.Error("CSV data file {file} does not exist, exiting", csvFile);
      return;
    }

    if (targetTime < startTime)
      targetTime += TimeSpan.FromDays(1);

    if (endTime < startTime)
      endTime += TimeSpan.FromDays(1);

    var targetDuraton = targetTime - startTime;
    var currentDuration = endTime - startTime;
    var k = targetDuraton.TotalSeconds / currentDuration.TotalSeconds;

    var sb = new StringBuilder("Location;Name;Arrival;Departure;Bold\r\n");
    foreach (var line in File.ReadLines(csvFile.FullName).Skip(1)) {
      var parts = line.Split(';');
      if (parts.Length != 5) {
        Log.Warning("Skipping invalid line: {line}", line);
        continue;
      }

      var location = parts[0];
      var name = parts[1];
      var bold = parts[4];

      Log.Information("Processing {stopName} ({stopLocation})...", name, location);

      TimeSpan? arrival = null;
      if (TimeSpan.TryParse(parts[2], out var arr)) {
        if (arr < startTime)
          arr += TimeSpan.FromDays(1);
        var interval = arr - startTime;
        arrival = startTime + k * interval;

        Log.Information("- Adjusted arrival time from {originalTime} to {adjustedTime}", arr, arrival);
      }

      TimeSpan? departure = null;
      if (TimeSpan.TryParse(parts[3], out var dep)) {
        if (dep < startTime)
          dep += TimeSpan.FromDays(1);
        var interval = dep - startTime;
        departure = startTime + k * interval;

        Log.Information("- Adjusted arrival time from {originalTime} to {adjustedTime}", dep, departure);
      }

      var arrivalStr = arrival?.ToString("hh':'mm':'ss") ?? "";
      var departureStr = departure?.ToString("hh':'mm':'ss") ?? "";
      sb.AppendLine($"{location};{name};{arrivalStr};{departureStr};{bold}");
    }

    Log.Information("Saving backup file");
    File.Copy(csvFile.FullName, $"{csvFile.FullName}.bak", true);

    Log.Information("Saving updated CSV file");
    File.WriteAllText(csvFile.FullName, sb.ToString());

    Log.Information("Adjustment complete!");
  }
}
