using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Navigation;
using vEBuLa.Extensions;
using vEBuLa.Models;
using vEBuLa.ViewModels;

namespace vEBuLa;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
  [DllImport("Kernel32")]
  public static extern void AllocConsole();

  [LibraryImport("Kernel32")]
  private static partial void AttachConsole();

  [LibraryImport("Kernel32")]
  private static partial void FreeConsole();

  internal static IHost? AppHost { get; private set; }
  private readonly IConfigurationRoot? _serilogConfig;
  internal static string ConfigFolder { get; private set; } = "config";
  internal static ProgramConfig Configuration { get; private set; }

  internal static T? GetService<T>() where T : notnull => AppHost is null ? default : AppHost.Services.GetService<T>();

  public App() {
    //AttachConsole();
    AllocConsole();

    IConfigurationBuilder configBuilder = new ConfigurationBuilder();
#pragma warning disable CS8604 // Mögliches Nullverweisargument.
    configBuilder.AddJsonStream(GetType().Assembly.GetManifestResourceStream(GetType().Namespace + ".serilog.json"));
#pragma warning restore CS8604 // Mögliches Nullverweisargument.
    _serilogConfig = configBuilder.Build();

    Log.Logger = new LoggerConfiguration()
      .ReadFrom.Configuration(_serilogConfig.ApplyTimestamp())
      .ApplyDestructuringRules()
      .CreateLogger();

    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);

    Log.Information("Application initialization");
    try {
      AppHost = Host.CreateDefaultBuilder()
            .UseSerilog()
            .Build();
    }
    catch (Exception ex) {
      Log.Fatal(ex, "An exception occurred during application initialization");
    }
  }

  protected override void OnStartup(StartupEventArgs e) {
    Log.Information("Application startup");
    try {
      base.OnStartup(e);
      if (AppHost is null) throw new Exception(".NET hosting failed to initialize");
      AppHost.Start();

      if (Environment.GetEnvironmentVariable("vEbulaConfigPath") is string configPath)
        ConfigFolder = configPath;
      Log.Information("Config Folder: {ConfigFolder}", new FileInfo(ConfigFolder).FullName);
      if (!Directory.Exists(ConfigFolder))
        Directory.CreateDirectory(ConfigFolder);

      Configuration = new ProgramConfig(Path.Combine(ConfigFolder, "EbulaConfig.cfg"));

      var Ebula = new EbulaVM(Configuration);

      var MainWindow = new MainWindow() {
        DataContext = Ebula
      };
      MainWindow.Show();

      if (Configuration.GlobalHotkeys)
        Ebula.SetHotkeys();
    }
    catch (Exception ex) {
      Log.Fatal(ex, "An exception occurred during application startup");
    }
  }


  protected override void OnExit(ExitEventArgs e) {
    AppHost?.Dispose();

    base.OnExit(e);
  }

  private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
    AppHost?.Dispose();
    Log.Fatal((Exception) e.ExceptionObject, "An unhandled exception occurred");
  }
}
