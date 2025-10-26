using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System;
using System.CommandLine;
using System.IO;
using System.Runtime.InteropServices;
using vEBuLa.Extensions;

namespace vEBuLa;

public static partial class Startup {
  [LibraryImport("Kernel32")]
  private static partial void AllocConsole();

  [LibraryImport("Kernel32")]
  private static partial void AttachConsole();

  [LibraryImport("Kernel32")]
  private static partial void FreeConsole();

  const int SW_HIDE = 0;
  const int SW_SHOW = 5;

  [STAThread]
  public static void Main(string[] args) {
    SetUpLogging();

    var rootCommand = Root();

    rootCommand.Subcommands.Add(KmlToEbula());

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

  private static RootCommand Root() {
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

  private static Command KmlToEbula() {
    var optInput = new Option<FileInfo?>("--input", "-i") {
      Description = "Path to the input KML file",
      Required = true,
      DefaultValueFactory = (_) => new FileInfo("input.kml")
    };

    var optOutput = new Option<DirectoryInfo?>("--output", "-o") {
      Description = "Path to the output directory for the Ebula configuration files",
      Required = false,
      DefaultValueFactory = (r) => r.GetValue(optInput)?.Directory
    };

    var optDryRun = new Option<bool>("--dry-run", "-d") {
      Description = "If set, the conversion will be simulated without writing any files",
      Required = false,
      DefaultValueFactory = (_) => false
    };

    var command = new Command("kml-to-ebula", "Convert a KML file to an Ebula configuration file") {
      optInput,
      optOutput,
      optDryRun
    };

    command.SetAction((r) => {
      AllocConsole();
      KmlToEbulaConvert.Run(
        r.GetValue(optInput)!,
        r.GetValue(optOutput)!,
        r.GetValue(optDryRun));
    });

    return command;
  }
}
