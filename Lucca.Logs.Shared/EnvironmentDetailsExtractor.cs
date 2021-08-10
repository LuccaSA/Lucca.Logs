using Microsoft.Extensions.Options;
using System;

namespace Lucca.Logs.Shared
{
    public class EnvironmentDetailsExtractor
    {
        private readonly IOptions<LuccaLoggerOptions> _options;
        public EnvironmentDetailsExtractor(IOptions<LuccaLoggerOptions> options)
        {
            _options = options;
        }

        public string AppName => _options.Value.ApplicationName;
        public string AppPool { get; } = Environment.GetEnvironmentVariable("APP_POOL_ID", EnvironmentVariableTarget.Process);
    }
}