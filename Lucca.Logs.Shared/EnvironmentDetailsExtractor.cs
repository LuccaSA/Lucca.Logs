using Microsoft.Extensions.Options;
using System;

namespace Lucca.Logs.Shared
{
    public class EnvironmentDetailsExtractor
    {
        private readonly LuccaLoggerOptions _options;
        public EnvironmentDetailsExtractor(IOptions<LuccaLoggerOptions> options)
        {
            _options = options.Value;
        }

        public string AppName => _options.ApplicationName!;
        public string AppPool { get; } = Environment.GetEnvironmentVariable("APP_POOL_ID", EnvironmentVariableTarget.Process);
    }
}