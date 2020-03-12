using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Datadog.Trace;

namespace Lucca.Logs.Abstractions
{
    public class LogExtractor
    {
        private readonly ILogDetailsExtractor[] _logDetailsExtractors;
        private readonly EnvironmentDetailsExtractor _environmentDetailsExtractor;

        public LogExtractor(IEnumerable<ILogDetailsExtractor> logDetailsExtractors, EnvironmentDetailsExtractor environmentDetailsExtractor)
        {
            _logDetailsExtractors = logDetailsExtractors.ToArray();
            _environmentDetailsExtractor = environmentDetailsExtractor;
        }

        public Dictionary<string, string> GatherData(bool isError)
        {
            var data = new Dictionary<string, string>(16);

            TryAdd(LogMeta._appName, _environmentDetailsExtractor.AppName);
            TryAdd(LogMeta._appPool, _environmentDetailsExtractor.AppPool);

            for (int i = 0; i < _logDetailsExtractors.Length; i++)
            {
                ILogDetailsExtractor extractor = _logDetailsExtractors[i];
                if (!extractor.CanExtract)
                {
                    continue;
                }
                TryAdd(LogMeta._warning, extractor.Warning);
                TryAdd(LogMeta._pageRest, extractor.PageRest);
                TryAdd(LogMeta._pageRest2, extractor.PageRest2);
                TryAdd(LogMeta._page, extractor.Page);
                TryAdd(LogMeta._verb, extractor.Verb);
                TryAdd(LogMeta._uri, extractor.UriPath);
                TryAdd(LogMeta._serverName, extractor.ServerName);
                TryAdd(LogMeta._correlationId, extractor.CorrelationId);
                TryAdd(LogMeta._hostAddress, extractor.HostAddress);
                TryAdd(LogMeta._userAgent, extractor.UserAgent);

                var traceId = CorrelationIdentifier.TraceId;
                if (traceId != 0)
                {
                    TryAdd(LogMeta._traceId, traceId.ToString(CultureInfo.InvariantCulture));
                    TryAdd(LogMeta._spanId, CorrelationIdentifier.SpanId.ToString(CultureInfo.InvariantCulture));
                }

                if (!isError)
                {
                    return data;
                }
                data.Add(LogMeta.RawPostedData, extractor.Payload);
                return data;
            }

            void TryAdd(string key, string value)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    data.Add(key, value);
                }
            }

            return data;
        }
    }
}