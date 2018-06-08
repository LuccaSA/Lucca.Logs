using Microsoft.Extensions.Logging;

namespace Lucca.Logs.Shared
{
    internal static class NLogHelper
    {
        /// <summary>
        /// Convert loglevel to NLog variant.
        /// </summary>
        /// <param name="logLevel">level to be converted.</param>
        /// <returns></returns>
        internal static NLog.LogLevel ToNLogLevel(this LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return NLog.LogLevel.Trace;
                case LogLevel.Debug:
                    return NLog.LogLevel.Debug;
                case LogLevel.Information:
                    return NLog.LogLevel.Info;
                case LogLevel.Warning:
                    return NLog.LogLevel.Warn;
                case LogLevel.Error:
                    return NLog.LogLevel.Error;
                case LogLevel.Critical:
                    return NLog.LogLevel.Fatal;
                case LogLevel.None:
                    return NLog.LogLevel.Off;
                default:
                    return NLog.LogLevel.Debug;
            }
        }
    }
}