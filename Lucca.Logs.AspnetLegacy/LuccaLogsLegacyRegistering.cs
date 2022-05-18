using Serilog;
using Serilog.Extensions.Logging;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using Lucca.Logs.Shared;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Serilog.Core;

namespace Lucca.Logs.AspnetLegacy
{
    public static class LuccaLogsLegacyRegistering
    {
        public static void AddLuccaLogs(this ServicesContainer servicesContainer)
        {
            servicesContainer.Add(typeof(IExceptionLogger), new LuccaExceptionLogger());
            servicesContainer.Replace(typeof(IExceptionHandler), new ExceptionHandler());
        }

        internal static ILogger BuildLogger(IServiceProvider sp)
        {
            var luccaLoggerOptions = sp.GetRequiredService<IOptions<LuccaLoggerOptions>>().Value;
            var logEventEnrichers = sp.GetRequiredService<IEnumerable<ILogEventEnricher>>();
            var logFilePath = luccaLoggerOptions.LogFilePath ?? "logs.txt";

            Logger.DefaultFactory = new SerilogLoggerFactory();
            var loggerConfiguration = new LoggerConfiguration();

            foreach (var logEventEnricher in logEventEnrichers)
            {
                loggerConfiguration.Enrich.With(logEventEnricher);
            }
            return loggerConfiguration
                .ConfigurationLuccaLogs(logFilePath)
                .CreateLogger();
        }
    }
}