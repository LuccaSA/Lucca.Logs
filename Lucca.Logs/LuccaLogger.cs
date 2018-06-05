using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NLog;
using StackExchange.Exceptional;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Lucca.Logs
{
    public class LuccaLogger : ILogger
    {
        private static readonly object _emptyEventId = default(EventId);    // Cache boxing of empty EventId-struct
        private static readonly object _zeroEventId = default(EventId).Id;  // Cache boxing of zero EventId-Value

        private Tuple<string, string, string> _eventIdPropertyNames;

        private readonly string _categoryName;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Logger _nloLogger;
        private readonly LuccaLoggerOptions _options;
        private readonly string _appName;

        public LuccaLogger(string categoryName, IHttpContextAccessor httpContextAccessor, Logger nloLogger, LuccaLoggerOptions options, string appName)
        {
            _categoryName = categoryName;
            _httpContextAccessor = httpContextAccessor;
            _nloLogger = nloLogger;
            _options = options;
            _appName = appName;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            NLog.LogLevel nLogLogLevel = logLevel.ToNLogLevel();
            bool isLogging = IsNlogEnabled(nLogLogLevel);

            if (!isLogging && (exception == null || !Exceptional.IsLoggingEnabled))
            {
                return;
            }

            bool isError = logLevel == LogLevel.Error || logLevel == LogLevel.Critical;

            Dictionary<string, string> customData = LuccaDataWrapper.GatherData(exception, _httpContextAccessor?.HttpContext?.Request, isError, _appName);

            Guid? guid = null;
            if (Exceptional.IsLoggingEnabled && exception != null)
            {
                guid = ExceptionalLog(exception, customData);
            }

            if (!isLogging)
            {
                return;
            }

            string message = formatter(state, exception);

            // Log to NLog
            LogEventInfo eventInfo = CreateNlogEventInfo(eventId, exception, nLogLogLevel, message);

            // Get cutom data and inject
            AppendLuccaData(guid, eventInfo, customData);

            _nloLogger.Log(eventInfo);
        }

        private void AppendLuccaData(Guid? guid, LogEventInfo eventInfo, Dictionary<string, string> customData)
        {

            foreach (KeyValuePair<string, string> kv in customData)
            {
                eventInfo.Properties[kv.Key] = kv.Value;
            }

            if (guid.HasValue)
            {
                eventInfo.Properties[LuccaDataWrapper.Link] = _options.GuidLink + guid.Value.ToString("N");
            }
        }

        private LogEventInfo CreateNlogEventInfo(EventId eventId, Exception exception, NLog.LogLevel nLogLogLevel, string message)
        {
            //message arguments are not needed as it is already checked that the loglevel is enabled.
            LogEventInfo eventInfo = LogEventInfo.Create(nLogLogLevel, _nloLogger.Name, message);
            eventInfo.Exception = exception;
            if (!_options.IgnoreEmptyEventId || eventId.Id != 0 || !string.IsNullOrEmpty(eventId.Name))
            {
                // Attempt to reuse the same string-allocations based on the current <see cref="NLogProviderOptions.EventIdSeparator"/>
                Tuple<string, string, string> eventIdPropertyNames = _eventIdPropertyNames ?? new Tuple<string, string, string>(null, null, null);
                string eventIdSeparator = _options.EventIdSeparator ?? string.Empty;
                if (!ReferenceEquals(eventIdPropertyNames.Item1, eventIdSeparator))
                {
                    // Perform atomic cache update of the string-allocations matching the current separator
                    eventIdPropertyNames = new Tuple<string, string, string>(
                        eventIdSeparator,
                        string.Concat("EventId", eventIdSeparator, "Id"),
                        string.Concat("EventId", eventIdSeparator, "Name"));
                    _eventIdPropertyNames = eventIdPropertyNames;
                }

                bool idIsZero = eventId.Id == 0;
                eventInfo.Properties[eventIdPropertyNames.Item2] = idIsZero ? _zeroEventId : eventId.Id;
                eventInfo.Properties[eventIdPropertyNames.Item3] = eventId.Name;
                eventInfo.Properties["EventId"] = idIsZero && eventId.Name == null ? _emptyEventId : eventId;
            }

            return eventInfo;
        }

        private Guid? ExceptionalLog(Exception exception, Dictionary<string, string> customData)
        {
            if (exception == null)
            {
                return null;
            }

            Error error;
            if (_httpContextAccessor.HttpContext != null)
            {
                error = exception.Log(_httpContextAccessor.HttpContext, _categoryName, false, customData, _appName);
            }
            else
            {
                error = exception.LogNoContext(_categoryName, false, customData, _appName);
            }

            return error?.GUID;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            NLog.LogLevel convertLogLevel = logLevel.ToNLogLevel();
            return IsNlogEnabled(convertLogLevel);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }
            return NestedDiagnosticsLogicalContext.Push(state);
        }

        /// <summary>
        /// Is logging enabled for this logger at this <paramref name="logLevel"/>?
        /// </summary>
        private bool IsNlogEnabled(NLog.LogLevel logLevel) => _nloLogger.IsEnabled(logLevel);
    }

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