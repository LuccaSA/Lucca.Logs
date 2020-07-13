using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Extensions;
using Microsoft.Extensions.Options;

namespace Lucca.Logs.Shared
{
    public class CloudEventExtractor : ILogDetailsExtractor
    {
        private readonly IOptions<LuccaLoggerOptions> _options;
        public CloudEventExtractor(IOptions<LuccaLoggerOptions> options)
        {
            _options = options;
        }

        public LogDetail CreateLogDetail()
        {
            CloudEvent cloudEvent = _options.Value.CloudEventAccessor();
            if (cloudEvent == null)
            {
                return new LogDetail { CanExtract = false };
            }
            return new LogDetail
            {
                CanExtract = cloudEvent != null,
                PageRest = cloudEvent.Id,
                PageRest2 = cloudEvent.Type,
                Page = cloudEvent.Subject,
                Verb = "Event",
                Uri = cloudEvent.Source.ToString(),
                ServerName = cloudEvent.Source.DnsSafeHost,
                HostAddress = string.Empty,
                UserAgent = cloudEvent.SpecVersion.ToString(),
                CorrelationId = cloudEvent.Extension<DistributedTracingExtension>()?.TraceParent ?? string.Empty,
                Payload = cloudEvent.Data.ToString(),
                Warning = string.Empty
            };
        }
    }
}