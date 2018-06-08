using System;
using Lucca.Logs.Shared;
using StackExchange.Exceptional;
using StackExchange.Exceptional.Internal;

namespace Lucca.Logs.AspnetCore
{
    public sealed class ExceptionalWrapperCore : IExceptionalWrapper
    {
        public bool Enabled => Exceptional.IsLoggingEnabled;
        public void Configure(Action<ExceptionalSettingsBase> configSettings)
        {
            Exceptional.Configure(configSettings);
        }
    }
}