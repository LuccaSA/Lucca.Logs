using System;
using CloudNative.CloudEvents;
using Lucca.Logs.Shared;
using Lucca.Logs.Shared.Opserver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;
using System.IO;
using System.Text;
#if NETCOREAPP3_1
using Microsoft.AspNetCore.Http;
#else
using Lucca.Logs.AspnetLegacy;
#endif

namespace Lucca.Logs.AspnetCore
{
    public static class LuccaLoggerExtensions
    {
        public static ILuccaLoggingBuilder AddLuccaLogs(this ILoggingBuilder loggingBuilder, IConfigurationSection config, string appName, Action<LuccaLoggerOptions> configureOptions = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            loggingBuilder.Services.AddLuccaLogs(config, appName, configureOptions);
            return new LuccaLoggingBuilder(loggingBuilder);
        }

        public static ILuccaLoggingBuilder AddLuccaLogs(this ILoggingBuilder loggingBuilder, Action<LuccaLoggerOptions> configureOptions, string appName, LogStore logStore = null)
        {
            loggingBuilder.Services.AddLuccaLogs(null, appName, configureOptions, logStore);
            return new LuccaLoggingBuilder(loggingBuilder);
        }

        public static ILoggingBuilder WithCloudEvents(this ILuccaLoggingBuilder luccaLoggingBuilder, Func<CloudEvent> cloudEventAccessor)
        {
            luccaLoggingBuilder.Services.PostConfigure<LuccaLoggerOptions>(o =>
            {
                o.CloudEventAccessor = cloudEventAccessor;
            });
            luccaLoggingBuilder.Services.AddSingleton<ILogDetailsExtractor, CloudEventExtractor>();
            return luccaLoggingBuilder;
        }

        private static IServiceCollection AddLuccaLogs(this IServiceCollection services, IConfigurationSection config, string appName, Action<LuccaLoggerOptions> configureOptions = null, LogStore logStore = null)
        {
            if (string.IsNullOrWhiteSpace(appName))
            {
                throw new ArgumentNullException(nameof(appName));
            }
  
            services.AddOptions();
            if (config != null)
            {
                if (!config.Exists())
                {
                    throw new LogConfigurationException("Missing configuration section");
                }
                services.Configure<LuccaLoggerOptions>(config);
            }
            
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }

            services.PostConfigure<LuccaLoggerOptions>(o =>
            {
                if (string.IsNullOrWhiteSpace(o.ApplicationName))
                {
                    o.ApplicationName = appName;
                }
            });
            services.RegisterLuccaLogsProvider(logStore);
            return services;
        }
         

        private static void RegisterLuccaLogsProvider(this IServiceCollection services, LogStore logStore = null)
        {
            services.AddSingleton<ILogDetailsExtractor, HttpLogDetailsExtractor>();
            services.AddSingleton<LuccaLogsEnricher>();
            services.AddSingleton<OpserverLogSinkAsync>();

#if NETCOREAPP3_1
            services.TryAddSingleton<IExceptionQualifier, GenericExceptionQualifier>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<IHttpContextParser, HttpContextParserCore>();
#else
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessorLegacy>();
            services.TryAddSingleton<IHttpContextParser, HttpContextParserLegacy>();
#endif

            services.AddSingleton<ILoggerProvider, SerilogLoggerProvider>(s =>
            {
                var enrich = s.GetRequiredService<LuccaLogsEnricher>();
                var asyncSink = s.GetRequiredService<OpserverLogSinkAsync>();
                var opt = s.GetRequiredService<IOptions<LuccaLoggerOptions>>().Value;

                string path = PrepareLogPath(opt);

                var loggerConf = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.With(enrich)
                    .WriteTo.Async(asyncSink, conf =>
                    {
                        conf.File(new JsonFormatter(), path, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 31, encoding: Encoding.UTF8);
                        conf.Console();
                    }, logStore).MinimumLevel.Is(LogEventLevel.Debug);

                return new SerilogLoggerProvider(loggerConf.CreateLogger(), true);
            });
        }

        private static string PrepareLogPath(LuccaLoggerOptions options)
        {
            string path;
            if (Path.IsPathRooted(options.LogFilePath))
            {
                path = options.LogFilePath;
            }
            else if (!string.IsNullOrWhiteSpace(options.LogFilePath))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, options.LogFilePath);
            }
            else
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "/logs/logfile.txt");
            }

            return path;
        }
    }

    public interface ILuccaLoggingBuilder : ILoggingBuilder
    {
    }

    public class LuccaLoggingBuilder : ILuccaLoggingBuilder
    {
        public LuccaLoggingBuilder(ILoggingBuilder loggingBuilder)
        {
            Services = loggingBuilder.Services;
        }

        public IServiceCollection Services { get; }
    }

}