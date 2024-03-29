using Lucca.Logs.Shared;
using Microsoft.Extensions.Options;

namespace Lucca.Logs.Netcore.Tests
{
    public class InjectOption
    {
        public InjectOption(IOptions<LuccaLoggerOptions> options)
        {
            Options = options.Value;
        }

        public LuccaLoggerOptions Options { get; }
    }
}