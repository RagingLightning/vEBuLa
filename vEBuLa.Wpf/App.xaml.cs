using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using vEBuLa.Wpf.Extensions;
using vEBuLa.Wpf.ViewModels;

namespace vEBuLa.Wpf;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
  [LibraryImport("Kernel32")]
  private static partial void AttachConsole();

  [LibraryImport("Kernel32")]
  private static partial void FreeConsole();

  private readonly IHost? _host;
  private readonly IConfigurationRoot? _serilogConfig;

  public App() {
    AttachConsole();

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
      _host = Host.CreateDefaultBuilder()
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
      if (_host is null) throw new Exception(".NET hosting failed to initialize");
      _host.Start();

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
    _host?.Dispose();

    base.OnExit(e);
  }

  private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
    _host?.Dispose();
    Log.Fatal((Exception) e.ExceptionObject, "An unhandled exception occurred");
  }
}
