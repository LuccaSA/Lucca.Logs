using System;
using StackExchange.Exceptional.Internal;

namespace Lucca.Logs.Shared
{
    public interface IExceptionalWrapper
    {
        bool Enabled { get; }
    }
}