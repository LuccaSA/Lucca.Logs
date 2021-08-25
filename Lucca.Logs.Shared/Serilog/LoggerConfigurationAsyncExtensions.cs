using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using System;

namespace Lucca.Logs.Shared
{
    public static class LoggerConfigurationAsyncExtensions
    {
        public static LoggerConfiguration Async(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            ILogEventSinkAsync additionalSinkAsync,
            Action<LoggerSinkConfiguration> configure, LogStore store)
        {
            return LoggerSinkConfiguration.Wrap(
                loggerSinkConfiguration,
                wrappedSink => new BackgroundWorkerSink(wrappedSink, additionalSinkAsync, store),
                configure,
                LevelAlias.Minimum,
                null);
        }
    }
}