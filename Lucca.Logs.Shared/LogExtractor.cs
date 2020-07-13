using System.Collections.Generic;
using System.Linq;
using Datadog.Trace;

namespace Lucca.Logs.Shared
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

                var logdetail = extractor.CreateLogDetail();
                if (!logdetail.CanExtract)
                {
                    continue;
                }
                TryAdd(LogMeta._warning, logdetail.Warning);
                TryAdd(LogMeta._pageRest, logdetail.PageRest);
                TryAdd(LogMeta._pageRest2, logdetail.PageRest2);
                TryAdd(LogMeta._page, logdetail.Page);
                TryAdd(LogMeta._verb, logdetail.Verb);
                TryAdd(LogMeta._uri, logdetail.Uri);
                TryAdd(LogMeta._serverName, logdetail.ServerName);
                TryAdd(LogMeta._correlationId, logdetail.CorrelationId);
                TryAdd(LogMeta._hostAddress, logdetail.HostAddress);
                TryAdd(LogMeta._userAgent, logdetail.UserAgent);

                var traceId = CorrelationIdentifier.TraceId;
                if (traceId != 0)
                {
                    TryAdd(LogMeta._traceId, traceId.ToString());
                    TryAdd(LogMeta._spanId, CorrelationIdentifier.SpanId.ToString());
                }

                if (!isError)
                {
                    return data;
                }
                data.Add(LogMeta.RawPostedData, logdetail.Payload);
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