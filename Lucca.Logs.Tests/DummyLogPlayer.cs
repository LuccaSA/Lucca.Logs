using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lucca.Logs.Tests
{
    public class DummyLogPlayer : ILogPlayer<DummyLogPlayer>
    {
        public ILogger<DummyLogPlayer> Logger { get; }

        public DummyLogPlayer(ILogger<DummyLogPlayer> logger)
        {
            Logger = logger;
        }
    }

    public class InjectOption
    {
        public InjectOption(IOptions<LuccaLoggerOptions> options)
        {
            Options = options.Value;
        }

        public LuccaLoggerOptions Options { get; }
    }
}