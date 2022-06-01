using Lucca.Logs.Shared;
using StackExchange.Exceptional;

namespace Lucca.Logs.AspnetLegacy
{
    public class ExceptionalWrapperLegacy : IExceptionalWrapper
    {
        public bool Enabled => Exceptional.IsLoggingEnabled;
    }
}
