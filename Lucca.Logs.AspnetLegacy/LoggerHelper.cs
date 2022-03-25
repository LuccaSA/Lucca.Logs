using System;
using Microsoft.Extensions.Logging;

namespace Lucca.Logs.AspnetLegacy
{
    public static class LoggerHelper
    {
        [Obsolete("appName is unused")]
        public static void LogException<T>(this T sourceInstance, Exception ex, string appName, string? message = null)
        {
            Logger.GetFactory.CreateLogger<T>().LogError(ex, message);
        }

        public static void LogException<T>(this T _, Exception ex, string? message = null)
        {
            Logger.GetFactory.CreateLogger<T>().LogError(ex, message);
        }
    }
}