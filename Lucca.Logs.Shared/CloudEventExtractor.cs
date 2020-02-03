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

        public bool CanExtract => Event != null;

        public string PageRest => Event.Id;
        public string PageRest2 => Event.Type;
        public string Page => Event.Subject;
        public string Verb => "Event";
        public string Uri => Event.Source.ToString();
        public string ServerName => Event.Source.DnsSafeHost;
        public string HostAddress { get; } = string.Empty;
        public string UserAgent => Event.SpecVersion.ToString();
        public string CorrelationId
        {
            get
            {
                string traceParent = Event.Extension<DistributedTracingExtension>()?.TraceParent;
                if (traceParent != null)
                {
                    return traceParent;
                }
                return string.Empty;
            }
        }
        public string Payload => Event.Data.ToString();
        public string Warning { get; } = string.Empty;

        private CloudEvent Event
        {
            get
            {
                try
                {
                    return _options.Value.CloudEventAccessor();
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}