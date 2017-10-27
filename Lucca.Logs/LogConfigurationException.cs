using System;

namespace Lucca.Logs
{
    public class LogConfigurationException : Exception
    {
        public LogConfigurationException(string message)
            : base(message)
        {
        }
    }
}