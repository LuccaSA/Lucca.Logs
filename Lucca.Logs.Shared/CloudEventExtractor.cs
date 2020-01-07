using System;
using CloudNative.CloudEvents;
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
        public string CorrelationId { get; } = string.Empty;
        public string Payload => Event.Data.ToString();
        public string Warning { get; } = string.Empty;
        public int Priority { get; } = 42;

        private CloudEvent Event => _options.Value.CloudEventAccessor();
    }
}   