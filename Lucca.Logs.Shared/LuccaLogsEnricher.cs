using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace Lucca.Logs.Shared
{
    public class LuccaLogsEnricher : ILogEventEnricher
    {
        private readonly LuccaLoggerOptions _options;
        private readonly IHttpContextParser _httpContextWrapper;
        private readonly LogExtractor _logExtractor;
        private readonly IExceptionalWrapper _exceptionalWrapper;
        private readonly IExceptionQualifier _filters;

        public LuccaLogsEnricher(LuccaLoggerOptions options, IHttpContextParser httpContextWrapper, LogExtractor logExtractor, IExceptionalWrapper exceptionalWrapper, IExceptionQualifier filters)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpContextWrapper = httpContextWrapper;
            _logExtractor = logExtractor;
            _exceptionalWrapper = exceptionalWrapper;
            _filters = filters;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            TryGetContext(logEvent, out var categoryName);

            bool isError = logEvent.Level == LogEventLevel.Error || logEvent.Level == LogEventLevel.Fatal;
            Dictionary<string, string> customData = _logExtractor.GatherData(isError);

            Guid? guid = null;
            if (_exceptionalWrapper.Enabled && logEvent.Exception != null && (_filters == null || _filters.LogToOpserver(logEvent.Exception)))
            {
                guid = _httpContextWrapper.ExceptionalLog(logEvent.Exception, customData, categoryName, _options.ApplicationName);
            }

            if (guid.HasValue)
            {
                string path = null;
                if (_options.GuidWithPlaceHolder)
                {
                    path = String.Format(_options.GuidLink, guid.Value.ToString("N"));
                }
                else
                {
                    path = _options.GuidLink + guid.Value.ToString("N");
                }
                logEvent.AddPropertyIfAbsent(new LogEventProperty(LogMeta.Link, new ScalarValue(path)));
            }

            foreach (var v in customData)
            {
                logEvent.AddPropertyIfAbsent(new LogEventProperty(v.Key, new ScalarValue(v.Value)));
            }
        }

        private static bool TryGetContext(LogEvent logEvent, out string context)
        {
            if (logEvent.Properties.TryGetValue(Constants.SourceContextPropertyName, out var propertyValue))
            {
                if (propertyValue is ScalarValue sv && sv.Value is string sourceContext)
                {
                    context = sourceContext;
                    return true;
                }
            }
            context = null;
            return false;
        }
    }
}