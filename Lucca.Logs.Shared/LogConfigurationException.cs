using System;
using System.Runtime.Serialization;

namespace Lucca.Logs.Shared
{
    [Serializable]
    public sealed class LogConfigurationException : Exception
    {
        public LogConfigurationException(string message)
            : base(message)
        {
        }

        private LogConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}