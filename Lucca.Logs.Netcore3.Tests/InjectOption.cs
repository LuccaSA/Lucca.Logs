using System;
using Lucca.Logs.Abstractions;
using Microsoft.Extensions.Options;

namespace Lucca.Logs.Netcore.Tests
{
    public class InjectOption
    {
        public InjectOption(IOptions<LuccaLoggerOptions> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            Options = options.Value;
        }

        public LuccaLoggerOptions Options { get; }
    }
}