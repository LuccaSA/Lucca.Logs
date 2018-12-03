using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lucca.Logs.AspnetLegacy
{
    /// <summary>
    /// Logger
    /// </summary>
    public static class Logger
    {
        private static readonly Lazy<ILoggerFactory> _defaultLogger = new Lazy<ILoggerFactory>(() =>
        {
            return LoggerBuilder.CreateLuccaLogsFactory(_ => { });
        });

        /// <summary>
        /// GetFactory
        /// </summary>
        public static ILoggerFactory GetFactory => DefaultFactory ?? _defaultLogger.Value;

        /// <summary>
        /// DefaultFactory
        /// </summary>
        public static ILoggerFactory DefaultFactory { get; set; }

        /// <summary>
        /// Logs an exception
        /// </summary>
        /// <param name="ex">The exception to log</param>
        /// <param name="appName">The current application name</param>
        /// <param name="message">Format string of the log message in message template format</param>
        public static void LogException(Exception ex, string appName, string message = null)
        {
            _defaultLogger.Value.CreateLogger(appName).LogError(ex, message);
        }

        /// <summary>
        /// Logs an information
        /// </summary>
        /// <param name="objectToSerializeAsMessage">The objet to log</param>
        /// <param name="appName">The current application name</param>
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