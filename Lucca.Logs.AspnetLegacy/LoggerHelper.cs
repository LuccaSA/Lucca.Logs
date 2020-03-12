using System;
using Microsoft.Extensions.Logging;

namespace Lucca.Logs.AspnetLegacy
{
    public static class LoggerHelper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Backward compat")]
        public static void LogException<T>(this T sourceInstance, Exception ex, string appName, string message = null)
        {
            Logger.GetFactory.CreateLogger<T>().LogError(ex, message);
        }
    }
}