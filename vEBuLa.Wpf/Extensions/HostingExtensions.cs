using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace vEBuLa.Wpf.Extensions;
static class HostingExtensions {

  public static IHostBuilder RegisterEBuLaServices(this IHostBuilder builder) {

    builder.ConfigureServices((hostCtx, services) => {
      services.AddTransient<ILogger>();
    });

    return builder;
  }
}
