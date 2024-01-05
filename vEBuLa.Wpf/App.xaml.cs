using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
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

      var MainWindow = new MainWindow() {
        DataContext = new EbulaVM(new Ebula())
      };
      MainWindow.Show();
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
