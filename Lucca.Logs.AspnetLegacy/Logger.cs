using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lucca.Logs.AspnetLegacy
{
    public static class Logger
    {
        private static readonly Lazy<ILoggerFactory> _defaultLogger = new Lazy<ILoggerFactory>(() =>
        {
            return LoggerBuilder.CreateLuccaLogsFactory(_ => { });
        });

        public static ILoggerFactory GetFactory => DefaultFactory ?? _defaultLogger.Value;

        public static ILoggerFactory DefaultFactory { get; set; }

        /// <summary>
        /// Logs an exception
        /// </summary>
        /// <param name="ex">The exception to log</param>
        /// <param name="appName">The current application name</param>
        public static void LogException(Exception ex, string appName, string message = null)
        {
            _defaultLogger.Value.CreateLogger(appName).LogError(ex, message);
        }

        public static void LogInfo(object objectToSerializeAsMessage, string appName)
        {
            LogInfo(JsonConvert.SerializeObject(objectToSerializeAsMessage), appName);
        }

        /// <summary>
        /// Logs a message
        /// </summary> 
        public static void LogInfo(string message, string appName)
        {
            _defaultLogger.Value.CreateLogger(appName).LogInformation(message);
        }
    }
}