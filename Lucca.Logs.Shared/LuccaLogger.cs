using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Lucca.Logs.Shared
{
    public class LuccaLogger : ILogger
    {
        private static readonly object _emptyEventId = default(EventId);    // Cache boxing of empty EventId-struct
        private static readonly object _zeroEventId = default(EventId).Id;  // Cache boxing of zero EventId-Value

        private Tuple<string, string, string> _eventIdPropertyNames;

        private readonly string _categoryName;
        private readonly IHttpContextParser _httpContextWrapper;
        private readonly Logger _nloLogger;
        private readonly LuccaLoggerOptions _options;
        private readonly IEnumerable<IExceptionQualifier> _filters;
        private readonly IExceptionalWrapper _exceptionalWrapper;
        private readonly string _appName;

        public LuccaLogger(string categoryName, IHttpContextParser httpContextAccessor, Logger nloLogger, LuccaLoggerOptions options, IEnumerable<IExceptionQualifier> filters, IExceptionalWrapper exceptionalWrapper, string appName)
        {
            _categoryName = categoryName;
            _httpContextWrapper = httpContextAccessor;
            _nloLogger = nloLogger;
            _options = options;
            _filters = filters;
            _exceptionalWrapper = exceptionalWrapper;
            _appName = appName;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            NLog.LogLevel nLogLogLevel = logLevel.ToNLogLevel();
            bool isLogging = IsNlogEnabled(nLogLogLevel, _nloLogger);

            if (!isLogging && (exception == null || !_exceptionalWrapper.Enabled))
            {
                return;
            }

            bool isError = logLevel == LogLevel.Error || logLevel == LogLevel.Critical;

            Dictionary<string, string> customData = LuccaDataWrapper.GatherData(exception, _httpContextWrapper, isError, _appName);

            Guid? guid = null;
            if (_exceptionalWrapper.Enabled && exception != null && (_filters == null || !_filters.Any() || _filters.Any(ef => ef.LogToOpserver(exception))))
            {
                guid = _httpContextWrapper.ExceptionalLog(exception, customData, _categoryName, _appName);
            }

            if (!isLogging)
            {
                return;
            }

            string message = formatter(state, exception);

            // Log to NLog
            LogEventInfo eventInfo = CreateNlogEventInfo(eventId, exception, nLogLogLevel, message);

            // Get cutom data and inject
            AppendLuccaData(guid, eventInfo, _options, customData);

            _nloLogger.Log(eventInfo);
        }

        private static void AppendLuccaData(Guid? guid, LogEventInfo eventInfo, LuccaLoggerOptions options, Dictionary<string, string> customData)
        {
            foreach (KeyValuePair<string, string> kv in customData)
            {
                eventInfo.Properties[kv.Key] = kv.Value;
            }

            if (guid.HasValue)
            {
                eventInfo.Properties[LuccaDataWrapper.Link] = options.GuidLink + guid.Value.ToString("N");
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


        /// <summary>
        /// Is logging enabled for this logger at this <paramref name="logLevel"/>?
        /// </summary>
        private static bool IsNlogEnabled(NLog.LogLevel logLevel, Logger nloLogger) => nloLogger.IsEnabled(logLevel);

        public bool IsEnabled(LogLevel logLevel)
        {
            return IsNlogEnabled(logLevel.ToNLogLevel(), _nloLogger);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }
            return NestedDiagnosticsLogicalContext.Push(state);
        }
    }
}