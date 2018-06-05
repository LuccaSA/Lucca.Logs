using System;

namespace Lucca.Logs.Shared
{
    public class LogConfigurationException : Exception
    {
        public LogConfigurationException(string message)
            : base(message)
        {
        }
    }
}