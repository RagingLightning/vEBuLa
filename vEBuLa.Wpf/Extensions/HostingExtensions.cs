using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace vEBuLa.Extensions;
static class HostingExtensions {

  public static IHostBuilder RegisterEBuLaServices(this IHostBuilder builder) {

    builder.ConfigureServices((hostCtx, services) => {
      services.AddTransient<ILogger>();
    });

    return builder;
  }
}
