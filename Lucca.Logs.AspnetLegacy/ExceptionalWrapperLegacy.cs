using System;
using Lucca.Logs.Abstractions;
using StackExchange.Exceptional;
using StackExchange.Exceptional.Internal;

namespace Lucca.Logs.AspnetLegacy
{
    public class ExceptionalWrapperLegacy : IExceptionalWrapper
    {
        public bool Enabled => Exceptional.IsLoggingEnabled;
        public void Configure(Action<ExceptionalSettingsBase> configSettings)
        {
            Exceptional.Configure(configSettings);
        }
    }
}
