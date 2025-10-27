using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System;
using System.CommandLine;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using vEBuLa.Extensions;

namespace vEBuLa;

public static partial class Startup {
  [LibraryImport("Kernel32")]
  private static partial void AllocConsole();

  [LibraryImport("Kernel32")]
  private static partial void AttachConsole();

  [LibraryImport("Kernel32")]
  private static partial void FreeConsole();

  [STAThread]
  public static void Main(string[] args) {
    SetUpLogging();

    var rootCommand = RootCmd();

    rootCommand.Subcommands.Add(ImportCmd());
    rootCommand.Subcommands.Add(ExportCmd());
    rootCommand.Subcommands.Add(ToolsCmd());

    rootCommand.Parse(args).Invoke();
  }

  private static void SetUpLogging() {
    IConfigurationBuilder configBuilder = new ConfigurationBuilder();
#pragma warning disable CS8604 // Mögliches Nullverweisargument.
    configBuilder.AddJsonStream(typeof(Startup).Assembly.GetManifestResourceStream(typeof(Startup).Namespace + ".serilog.json"));
#pragma warning restore CS8604 // Mögliches Nullverweisargument.

    Log.Logger = new LoggerConfiguration()
      .ReadFrom.Configuration(configBuilder.Build().ApplyTimestamp())
      .ApplyDestructuringRules()
      .CreateLogger();
  }

  private static RootCommand RootCmd() {
    var optConsole = new Option<bool>("--console", "-c") {
      Description = "Show log output in a console window",
      Required = false,
      DefaultValueFactory = (_) => false
    };

    var rootCommand = new RootCommand("vEBuLa Command Line Interface") {
      optConsole
    };

    rootCommand.SetAction((r) => {
      if (r.GetValue(optConsole))
        AllocConsole();

      App.Main();
    });

    return rootCommand;
  }

  private static Command ImportCmd() {
    var command = new Command("import", "Import data into an EBuLa configuration file");

    command.Subcommands.Add(ImportEntriesFromKml());
    command.Subcommands.Add(ImportStopsFromCsv());

    return command;
  }

  private static Command ImportEntriesFromKml() {
    var optInput = new Option<FileInfo>("--input", "-i") {
      Description = "Path to the input KML file",
      Required = true,
      DefaultValueFactory = (_) => new FileInfo("input.kml")
    };

    var optOutput = new Option<DirectoryInfo>("--output", "-o") {
      Description = "Path to the output directory for the EBuLa configuration files",
      Required = false,
      DefaultValueFactory = (r) => r.GetValue(optInput)!.Directory ?? new DirectoryInfo(".")
    };

    var optDryRun = new Option<bool>("--dry-run", "-d") {
      Description = "If set, the conversion will be simulated without writing any files",
      Required = false,
      DefaultValueFactory = (_) => false
    };

    var command = new Command("kml", "Convert a KML file to an EBuLa configuration file") {
      optInput,
      optOutput,
      optDryRun
    };

    command.SetAction((r) => {
      AllocConsole();
      Tools.Import.EntriesFromKml.Run(
        r.GetValue(optInput)!,
        r.GetValue(optOutput)!,
        r.GetValue(optDryRun));
//#if DEBUG
//      new ManualResetEvent(false).WaitOne();
//#endif
    });

    return command;
  }

  private static Command ImportStopsFromCsv() {
    var optConfig = new Option<FileInfo>("--config", "-c") {
      Description = "Path to the EBuLa configuration",
      Required = true,
      DefaultValueFactory = (_) => new FileInfo("input.ebula")
    };

    var optServiceNumber = new Option<string>("--service", "-s") {
      Description = "Service number to import stops for",
      Required = true,
      DefaultValueFactory = (_) => "0"
    };

    var optInputCsv = new Option<FileInfo>("--input", "-i") {
      Description = "Path to the input CSV file",
      Required = false,
      DefaultValueFactory = (r) => {
        var inputDir = r.GetValue(optConfig)!.DirectoryName;
        var inputName = r.GetValue(optConfig)!.Name;
        var serviceNumber = r.GetValue(optServiceNumber)!;
        var csvName = inputName.Replace(".ebula", $"_{serviceNumber}.stops.csv");

        return new FileInfo(Path.Combine(inputDir!, csvName));
      }
    };

    var optTrim = new Option<bool>("--trim", "-t") {
      Description = "Remove stops not included in the CSV file from the service",
      Required = false,
      DefaultValueFactory = (_) => false
    };

    var command = new Command("stops", "Import stops of a service from a csv file") {
      optConfig,
      optInputCsv,
      optServiceNumber,
      optTrim
    };

    command.SetAction((r) => {
      AllocConsole();
      Tools.Import.StopsFromCsv.Run(
        r.GetValue(optConfig)!,
        r.GetValue(optInputCsv)!,
        r.GetValue(optServiceNumber)!,
        r.GetValue(optTrim));
//#if DEBUG
//      new ManualResetEvent(false).WaitOne();
//#endif
    });

    return command;
  }

  private static Command ExportCmd() {
    var command = new Command("export", "Export data from an EBuLa configuration file");

    command.Subcommands.Add(ExportStopsToCsv());

    return command;

  }

  private static Command ExportStopsToCsv() {
    var optInput = new Option<FileInfo>("--config", "-c") {
      Description = "Path to the EBuLa configuration",
      Required = true,
      DefaultValueFactory = (_) => new FileInfo("input.ebula")
    };

    var optServiceNumber = new Option<string>("--service", "-s") {
      Description = "Service number to export stops for",
      Required = true,
      DefaultValueFactory = (_) => "0"
    };

    var optOutput = new Option<FileInfo>("--output", "-o") {
      Description = "Path to the output file",
      Required = false,
      DefaultValueFactory = (r) => {
        var inputDir = r.GetValue(optInput)!.DirectoryName;
        var inputName = r.GetValue(optInput)!.Name;
        var serviceNumber = r.GetValue(optServiceNumber)!;
        var outputName = inputName.Replace(".ebula", $"_{serviceNumber}.stops.csv");

        return new FileInfo(Path.Combine(inputDir!, outputName));
      }
    };

    var command = new Command("stops", "Export all stops of a service to a csv file") {
      optInput,
      optOutput,
      optServiceNumber
    };

    command.SetAction((r) => {
      AllocConsole();
      Tools.Export.StopsToCsv.Run(
        r.GetValue(optInput)!,
        r.GetValue(optOutput)!,
        r.GetValue(optServiceNumber)!);
//#if DEBUG
//      new ManualResetEvent(false).WaitOne();
//#endif
    });

    return command;
  }

  private static Command ToolsCmd() {
    var command = new Command("tools", "Tools for miscellaneous files");

    command.Subcommands.Add(AdjustStopTimes());
    return command;
  }

  private static Command AdjustStopTimes() {
    var optInput = new Option<FileInfo>("--input", "-i") {
      Description = "Path to the CSV stop data file",
      Required = true,
      DefaultValueFactory = (_) => new FileInfo("service.stops.csv")
    };

    var optStart = new Option<TimeSpan>("--start-time", "-s") {
      Description = "Start time of the segment",
      Required = true,
      DefaultValueFactory = (_) => TimeSpan.Zero
    };

    var optEnd = new Option<TimeSpan>("--end-time", "-e") {
      Description = "End time of the segment",
      Required = true,
      DefaultValueFactory = (_) => TimeSpan.Zero
    };

    var optTarget = new Option<TimeSpan>("--target-time", "-t") {
      Description = "Target end time of the segment",
      Required = true,
      DefaultValueFactory = (_) => TimeSpan.Zero
    };

    var command = new Command("adjust-times", "Adjust times of stops in a csv file to fit target time frame") {
      optInput,
      optStart,
      optEnd,
      optTarget
    };

    command.SetAction((r) => {
      AllocConsole();
      Tools.AdjustStopTimes.Run(
        r.GetValue(optInput)!,
        r.GetValue(optStart)!,
        r.GetValue(optEnd)!,
        r.GetValue(optTarget));
//#if DEBUG
//      new ManualResetEvent(false).WaitOne();
//#endif
    });

    return command;
  }
}
