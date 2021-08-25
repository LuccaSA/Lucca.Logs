using Lucca.Logs.Shared.Exceptional;
using Microsoft.Extensions.Options;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lucca.Logs.Shared
{
    public class LuccaLogsEnricher : ILogEventEnricher
    {
        private readonly IOptions<LuccaLoggerOptions> _options;
        private readonly IExceptionQualifier _filters;
        private readonly ILogDetailsExtractor[] _logDetailsExtractors;

        public LuccaLogsEnricher(IOptions<LuccaLoggerOptions> options,
            IExceptionQualifier filters,
            IEnumerable<ILogDetailsExtractor> logDetailsExtractors)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _filters = filters ?? throw new ArgumentNullException(nameof(filters));
            if (logDetailsExtractors == null) throw new ArgumentNullException(nameof(logDetailsExtractors));

            _logDetailsExtractors = logDetailsExtractors.ToArray();
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            // todo :
            // - plug IExceptionQualifier

            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            bool isError = logEvent.Level == LogEventLevel.Error || logEvent.Level == LogEventLevel.Fatal;

            logEvent.TryAdd(LogMeta.AppName, _options.Value.ApplicationName);
            logEvent.TryAdd(LogMeta.AppPool, Environment.GetEnvironmentVariable("APP_POOL_ID", EnvironmentVariableTarget.Process));

            if (logEvent.Exception != null)
            {
                logEvent.TryAdd(LogMeta.LogSiteStackTrace, $"\n\nFull Trace:\n\n{new StackTrace(3, true)}");
                var exception = logEvent.Exception.ToString();
                logEvent.TryAdd(LogMeta.StackTrace, exception);
                logEvent.TryAdd(LogMeta.ExceptionHash, GetHashcode(exception).ToString());

                var baseException = logEvent.Exception;
                if (baseException.IsBCLException())
                    baseException = baseException.GetBaseException();

                logEvent.TryAdd(LogMeta.Exception, exception); // todo : en doublon avec StackTrace
                logEvent.TryAdd(LogMeta.ExceptionType, baseException.GetType().FullName);
                logEvent.TryAdd(LogMeta.ExceptionMessage, baseException.Message);
                logEvent.TryAdd(LogMeta.ExceptionSource, baseException.Source);
            }

            for (int i = 0; i < _logDetailsExtractors.Length; i++)
            {
                ILogDetailsExtractor extractor = _logDetailsExtractors[i];

                var logdetail = extractor.CreateLogDetail(logEvent.Exception != null);
                if (!logdetail.CanExtract)
                {
                    continue;
                }
                logEvent.TryAdd(LogMeta.Warning, logdetail.Warning);
                logEvent.TryAdd(LogMeta.PageRest, logdetail.PageRest);
                logEvent.TryAdd(LogMeta.PageRest2, logdetail.PageRest2);
                logEvent.TryAdd(LogMeta.Page, logdetail.Page);
                logEvent.TryAdd(LogMeta.Verb, logdetail.Verb);
                logEvent.TryAdd(LogMeta.Uri, logdetail.Uri);
                logEvent.TryAdd(LogMeta.ServerName, logdetail.ServerName);
                logEvent.TryAdd(LogMeta.CorrelationId, logdetail.CorrelationId);
                logEvent.TryAdd(LogMeta.HostAddress, logdetail.HostAddress);
                logEvent.TryAdd(LogMeta.UserAgent, logdetail.UserAgent);


                if (isError)
                {
                    logEvent.TryAdd(LogMeta.RawPostedData, logdetail.Payload);
                }
            }
        }

        private static int GetHashcode(string exception)
        {
            int hashcode;
            unchecked
            {
                hashcode = (exception.GetDeterministicHashCode() * 397)
                           ^ Environment.MachineName.GetDeterministicHashCode();
            }
            return hashcode;
        }
    }
}