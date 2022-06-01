using Serilog;
using Serilog.Formatting.Json;
using System;

namespace Lucca.Logs.Shared
{
    public static class LoggerConfigurationExtensions
    {
        public static LoggerConfiguration ConfigurationLuccaLogs(this LoggerConfiguration loggerConfiguration, string logPath)
        {
            return loggerConfiguration
                    .Enrich.FromLogContext()
                    .WriteTo.File(
                           new JsonFormatter(renderMessage: true),
                           logPath,
                           rollingInterval: RollingInterval.Day,
                           retainedFileCountLimit: 31,
                           rollOnFileSizeLimit: true,
                           flushToDiskInterval: TimeSpan.FromSeconds(1)
                   )
                   .WriteTo.Console();
        }


    }
}
