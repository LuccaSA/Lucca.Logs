using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;

namespace Lucca.Logs
{
    public static class LuccaLoggerExtensions
    {
        public static ILoggingBuilder AddLuccaLogs(this ILoggingBuilder loggingBuilder, IConfigurationSection config, Action<LuccaLoggerOptions> configureOptions = null)
        {
            loggingBuilder.Services.AddLuccaLogs(config, configureOptions);
            return loggingBuilder;
        }

        public static ILoggingBuilder AddLuccaLogs(this ILoggingBuilder loggingBuilder, Action<LuccaLoggerOptions> configureOptions)
        {
            loggingBuilder.Services.AddLuccaLogs(configureOptions);
            return loggingBuilder;
        }

        public static IServiceCollection AddLuccaLogs(this IServiceCollection services, IConfigurationSection config, Action<LuccaLoggerOptions> configureOptions = null)
        {
            if (!config.Exists())
            {
                throw new LogConfigurationException("Missing configuration section");
            }
            services.AddOptions();
            services.Configure<LuccaLoggerOptions>(config);
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }
            RegisterProvider(services);
            return services;
        }

        public static IServiceCollection AddLuccaLogs(this IServiceCollection services, Action<LuccaLoggerOptions> configureOptions)
        {
            services.AddOptions();
            services.Configure(configureOptions);
            RegisterProvider(services);
            return services;
        }

        /// <summary>
        /// Apply NLog configuration from XML config.
        /// </summary>
        /// <param name="fileName">absolute path  NLog configuration file.</param>
        /// <returns>LoggingConfiguration for chaining</returns>
        private static LoggingConfiguration ConfigureNLog(string fileName)
        {
            var configuration = new XmlLoggingConfiguration(fileName, true);
            LogManager.Configuration = configuration;
            return configuration;
        }

        private static void RegisterProvider(IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ILoggerProvider, LuccaLogsProvider>();
        }
    }


}