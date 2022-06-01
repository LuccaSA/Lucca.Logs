using Microsoft.Extensions.Options;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lucca.Logs.Shared
{
    internal class LuccaLoggerEnricher : ILogEventEnricher
    {
        private const string GuidFormat = "N";

        private readonly LogExtractor _logExtractor;
        private readonly LuccaLoggerOptions _options;
        private readonly IExceptionalWrapper _exceptionalWrapper;
        private readonly IExceptionQualifier _filters;
        private readonly IHttpContextParser _httpContextWrapper;
        private readonly string _appName;

        public LuccaLoggerEnricher(
            LogExtractor logExtractor, IOptions<LuccaLoggerOptions> options,
            IExceptionalWrapper exceptionalWrapper, IExceptionQualifier filters,
            IHttpContextParser httpContextWrapper)
        {
            _logExtractor = logExtractor;
            _options = options.Value;
            _exceptionalWrapper = exceptionalWrapper;
            _filters = filters;
            _httpContextWrapper = httpContextWrapper;
            _appName = _options.ApplicationName ?? "";
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var categoryName = "unknown";
            if (logEvent.Properties.TryGetValue("SourceContext", out var propertyCategoryName))
            {
                var sb = new StringBuilder();
                using (var tw = new StringWriter(sb))
                {
                    propertyCategoryName.Render(tw);
                }
                categoryName = sb.ToString();
            }
            bool isError = logEvent.Level == LogEventLevel.Error || logEvent.Level == LogEventLevel.Fatal;

            Dictionary<string, string?> customData = _logExtractor.GatherData(isError);

            Guid? guid = null;
            var exception = logEvent.Exception;
            if (_exceptionalWrapper.Enabled && exception is not null && (_filters is null || _filters.LogToOpserver(exception)))
            {
                guid = _httpContextWrapper.ExceptionalLog(exception, customData, categoryName, _appName);
            }

            AppendLuccaData(guid, logEvent, _options, customData, propertyFactory);
        }

        private static void AppendLuccaData(Guid? guid, LogEvent eventInfo, LuccaLoggerOptions options, Dictionary<string, string?> customData, ILogEventPropertyFactory propertyFactory)
        {
            foreach (KeyValuePair<string, string?> kv in customData)
            {
                if (kv.Key != LogMeta.RawPostedData)
                {
                    var property = propertyFactory.CreateProperty(kv.Key, kv.Value);
                    eventInfo.AddPropertyIfAbsent(property);
                }
            }

            if (!guid.HasValue)
            {
                return;
            }

            if (options.GuidWithPlaceHolder)
            {
                var property = propertyFactory.CreateProperty(LogMeta.Link, string.Format(options.GuidLink, guid.Value.ToString(GuidFormat)));
                eventInfo.AddPropertyIfAbsent(property);
            }
            else
            {
                var property = propertyFactory.CreateProperty(LogMeta.Link, options.GuidLink + guid.Value.ToString(GuidFormat));
                eventInfo.AddPropertyIfAbsent(property);
            }
        }

    }
}
