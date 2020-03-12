using Microsoft.Extensions.Logging;

namespace Lucca.Logs.Netcore.Tests
{
    public class DummyLogPlayer : ILogPlayer<DummyLogPlayer>
    {
        public ILogger<DummyLogPlayer> Logger { get; }

        public DummyLogPlayer(ILogger<DummyLogPlayer> logger)
        {
            Logger = logger;
        }
    }
}