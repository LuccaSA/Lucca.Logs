using System;
using Microsoft.Extensions.Logging;

namespace Lucca.Logs.AspnetLegacy
{
    /// <summary>
    /// logger helper
    /// </summary>
    public static class LoggerHelper
    {
        /// <summary>
        /// Creates a new ILogger instance using the full name of the given type and formats and writes an error log message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ex">exception to log</param>
        /// <param name="message">message</param>
        public static void LogException<T>(Exception ex, string message = null)
        {
            Logger.GetFactory.CreateLogger<T>().LogError(ex, message);
        }
    }
}