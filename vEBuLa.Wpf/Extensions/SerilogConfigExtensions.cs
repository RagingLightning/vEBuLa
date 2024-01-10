using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Linq;
using vEBuLa.Models;
using vEBuLa.ViewModels;

namespace vEBuLa.Extensions;
internal static class SerilogConfigExtensions {
  private const string fileSection = "Serilog:WriteTo:FileSink:Args:path";
  private const string clefSection = "Serilog:WriteTo:ClefSink:Args:path";

  public static IConfigurationRoot ApplyTimestamp(this IConfigurationRoot config) {
    string timestamp = DateTime.Now.ToString("yyMMdd_HHmmss");
    if (config.GetSection(fileSection).Exists())
      config[fileSection] += $"{timestamp}.log";
    if (config.GetSection(clefSection).Exists())
      config[clefSection] += $"{timestamp}.clef";
    return config;
  }

  public static void CleanFolder(this IConfigurationRoot config, int filesToKeep) {
    try {
      if (config.GetSection(fileSection).Exists() && !string.IsNullOrEmpty(config[fileSection]))
        foreach (var fi in new DirectoryInfo(Path.GetDirectoryName(config[fileSection].Replace("%temp%", Environment.GetEnvironmentVariable("temp")))).EnumerateFiles("*.log").OrderByDescending(f => f.LastWriteTime).Skip(filesToKeep))
          fi.Delete();
    }
    catch (Exception) { }

    try {
      if (config.GetSection(clefSection).Exists() && !string.IsNullOrEmpty(config[clefSection]))
        foreach (var fi in new DirectoryInfo(Path.GetDirectoryName(config[clefSection].Replace("%temp%", Environment.GetEnvironmentVariable("temp")))).EnumerateFiles("*.clef").OrderByDescending(f => f.LastWriteTime).Skip(filesToKeep))
          fi.Delete();
    }
    catch (Exception) { }
  }

  public static LoggerConfiguration ApplyDestructuringRules(this LoggerConfiguration config) {
    return config
      /* General */
      .Destructure.ByTransforming<Guid>((guid) => guid.ToString().BiCrop(5,5))
      /* Models */
      .Destructure.ByTransforming<Ebula>(m => new { Config = m.Config, Departure = m.ServiceStartTime })
      .Destructure.ByTransforming<EbulaConfig>(m => new { Name = m.Name, Routes = m.Routes.Values, Segments = m.Segments.Values, Stations = m.Stations.Values })
      .Destructure.ByTransforming<EbulaEntry>(m => new { SpeedLimit = m.SpeedLimit, Location = m.Location, Label = m.LocationName })
      .Destructure.ByTransforming<EbulaRoute>(m => new { Id = m.Id, Name = m.Name, Route = m.RouteOverview, Segments = m.Segments.Select(s => s.Id) })
      .Destructure.ByTransforming<EbulaSegment>(m => new { Id = m.Id, Name = m.Name, Origin = m.Origin.Key, Destination = m.Destination.Key, PreEntries = m.PreEntries, MainEntries = m.Entries, PostEntries = m.PostEntries })
      .Destructure.ByTransforming<EbulaStation>(m => new { Id = m.Id, Name = m.Name })
      /* ViewModels */
      .Destructure.ByTransforming<EbulaCustomEntryVM>(vm => new { Origin = vm.Origin ?? vm.SelectedOrigin, Segment = vm.SelectedSegment, Destination = vm.Destination ?? vm.SelectedDestination })
      .Destructure.ByTransforming<EbulaEntryVM>(vm => new { Model = vm.Model })
      .Destructure.ByTransforming<EbulaMarkerEntryVM>(vm => new { Segment = vm.Segment, MarkerType = vm.MarkerType })
      .Destructure.ByTransforming<EbulaRouteEntryVM>(vm => new { Station = vm.Name, Departure = vm.Departure })
      .Destructure.ByTransforming<EbulaRouteVM>(vm => new { Model = vm.Model })
      .Destructure.ByTransforming<EbulaScreenVM>(vm => new { StartEntry = vm.StartEntry, CurrentEntry = vm.CurrentEntry, Entries = vm.Entries, ActiveEntries = vm.ActiveEntries })
      .Destructure.ByTransforming<EbulaSegmentVM>(vm => new { Model = vm.Model })
      .Destructure.ByTransforming<EbulaStationVM>(vm => new {Model = vm.Model})
      .Destructure.ByTransforming<EbulaVM>(vm => new {Model = vm.Model, Screen = vm.Screen, Active = vm.Active, EditMode = vm.EditMode})
      .Destructure.ByTransforming<SetupScreenVM>(vm => new {UsingCustom = vm.UsingCustom, CustomRoute = vm.CustomRoute, PredefinedRoute = vm.SelectedRoute, Departure = vm.Departure});
  }
}
