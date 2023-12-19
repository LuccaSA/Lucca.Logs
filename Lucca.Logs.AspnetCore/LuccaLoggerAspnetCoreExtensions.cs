using Lucca.Logs.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System;
using System.IO;

namespace Lucca.Logs.AspnetCore
{
    public static class LuccaLoggerAspnetCoreExtensions
    {
        public static IApplicationBuilder UseLuccaLogs(this IApplicationBuilder app, LuccaExceptionHandlerOption? exceptionHandlerOption = null, bool enableContentLog = true)
        {
            var builder = app;
            if (enableContentLog)
            {
                builder = builder.UseMiddleware<EnableRequestContentRewindMiddleware>();
            }

            app.UseMiddleware<LuccaExceptionHandlerMiddleware>(Options.Create(exceptionHandlerOption ?? new LuccaExceptionHandlerOption()));

            return builder;
        }

        public static void InitLuccaLogs()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();
        }

        public static IHostBuilder UseLuccaLogs(this IHostBuilder hostBuilder)
        {
            hostBuilder
                .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .ConfigurationLuccaLogs(PrepareLogPath(services)));
            return hostBuilder;
        }

        private static string PrepareLogPath(IServiceProvider services)
        {
            var options = services.GetRequiredService<IOptions<LuccaLoggerOptions>>().Value;
            if (Path.IsPathRooted(options.LogFilePath))
            {
                return options.LogFilePath;
            }
            if (string.IsNullOrWhiteSpace(options.LogFilePath))
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs/logfile.txt");
            }
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, options.LogFilePath);
        }

    }
}
