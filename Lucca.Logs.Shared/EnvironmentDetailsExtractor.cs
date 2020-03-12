using System;

namespace Lucca.Logs.Abstractions
{
    public class EnvironmentDetailsExtractor
    {
        private readonly LuccaLoggerOptions _options;
        public EnvironmentDetailsExtractor(LuccaLoggerOptions options)
        {
            _options = options;
        }

        public string AppName => _options.ApplicationName;
        public string AppPool { get; } = Environment.GetEnvironmentVariable("APP_POOL_ID", EnvironmentVariableTarget.Process);
    }
}