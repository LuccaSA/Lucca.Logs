using Serilog;
using Serilog.Formatting.Compact;
using System;

namespace Lucca.Logs.Shared
{
    public static class LoggerConfigurationExtensions
    {
        public static LoggerConfiguration ConfigurationLuccaLogs(this LoggerConfiguration loggerConfiguration, string logPath)
        {
            return loggerConfiguration
                    .Enrich.FromLogContext()
                    .WriteTo.Async(sinkConfiguration =>
                    {
                        sinkConfiguration.File(
                           new RenderedCompactJsonFormatter(),
                           logPath,
                           rollingInterval: RollingInterval.Day,
                           retainedFileCountLimit: 31,
                           rollOnFileSizeLimit: true,
                           flushToDiskInterval: TimeSpan.FromSeconds(1)
                       );
                        sinkConfiguration.Console();
                    });
        }


    }
}
