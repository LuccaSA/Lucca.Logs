using System;
using StackExchange.Exceptional.Internal;

namespace Lucca.Logs.Abstractions
{
    public interface IExceptionalWrapper
    {
        bool Enabled { get; }
        void Configure(Action<ExceptionalSettingsBase> configSettings);
    }
}