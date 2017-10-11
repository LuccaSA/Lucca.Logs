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
		private readonly Logger _logger;
		private readonly LuccaLoggerOptions _options;
        private readonly string _appName;

		public LuccaLogger(string categoryName, IHttpContextAccessor httpContextAccessor, Logger logger, LuccaLoggerOptions options, string appName)
		{
			_categoryName = categoryName;
			_httpContextAccessor = httpContextAccessor;
			_logger = logger;
			_options = options;
            _appName = appName;
        }

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Guid? guid = ExceptionalLog(exception);

            // Log to NLog
            NLog.LogLevel nLogLogLevel = logLevel.ToNLogLevel();
            if (!IsNlogEnabled(nLogLogLevel))
            {
                return;
            }
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }
            string message = formatter(state, exception);

            LogEventInfo eventInfo = CreateNlogEventInfo(eventId, exception, nLogLogLevel, message);

            // Get cutom data and inject
            AppendLuccaData(exception, guid, eventInfo);

            _logger.Log(eventInfo);
        }

        private void AppendLuccaData(Exception exception, Guid? guid, LogEventInfo eventInfo)
        {
            Dictionary<string, string> customData = LuccaDataWrapper.GatherData(exception, _httpContextAccessor?.HttpContext?.Request, _appName);

            foreach (KeyValuePair<string, string> kv in customData)
            {
                eventInfo.Properties[kv.Key] = kv.Value;
            }

            if (guid.HasValue)
                eventInfo.Properties[LuccaDataWrapper.Link] = "http://opserver.lucca.local/exceptions/detail?id=" + guid.Value.ToString("N");
        }

        private LogEventInfo CreateNlogEventInfo(EventId eventId, Exception exception, NLog.LogLevel nLogLogLevel, string message)
        {
            //message arguments are not needed as it is already checked that the loglevel is enabled.
            LogEventInfo eventInfo = LogEventInfo.Create(nLogLogLevel, _logger.Name, message);
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

        private Guid? ExceptionalLog(Exception exception)
        {
            if (exception == null)
            {
                return null;
            }
            // Log exceptions to exceptional
            var dic = new Dictionary<string, string>();
            Error error = exception.Log(_httpContextAccessor.HttpContext, _categoryName, false, dic, _appName);
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
		private bool IsNlogEnabled(NLog.LogLevel logLevel)
			=> _logger.IsEnabled(logLevel);
    }

    internal static class NlogHelper
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