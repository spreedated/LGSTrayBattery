using LGSBackgroundService.Worker;
using LGSTrayCore;
using LGSTrayCore.Managers;
using LGSTrayPrimitives.IPC;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using static LGSTrayPrimitives.Constants;

namespace LGSBackgroundService
{
    internal static class Program
    {
        private readonly static LogEventLevel minimumLevel = LogEventLevel.Verbose;

        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Debug(restrictedToMinimumLevel: minimumLevel)
                .Enrich.WithProperty("Application", typeof(Program).Assembly.GetName().Name)
                .CreateLogger();

            Microsoft.Extensions.Logging.ILogger logger = new SerilogLoggerProvider().CreateLogger("app");

            logger.LogInformation("Starting up");

            HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(null);

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger);

            builder.Services.AddLGSMessagePipe(true);

            builder.Services.AddIDeviceManager<LGsTrayHidManager>(builder.Configuration);
            builder.Services.AddIDeviceManager<GHubManager>(builder.Configuration);

            builder.Services.AddHostedService<SharedMemoryDeviceServiceWorker>();

            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = BACKGROUND_SERVICE_NAME;
            });

            IHost host = builder.Build();
            host.Run();
        }
    }
}