using Lucca.Logs.Shared;
using StackExchange.Exceptional;

namespace Lucca.Logs.AspnetCore
{
    public sealed class ExceptionalWrapperCore : IExceptionalWrapper
    {
        public bool Enabled => Exceptional.IsLoggingEnabled;
    }
}