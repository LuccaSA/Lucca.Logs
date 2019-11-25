using System;
using System.Collections.Generic;
using System.Text;

namespace Lucca.Logs.Shared
{
    public class EmptyContextParser : IContextParser
    {
        public virtual bool CanRead() => true;

        public virtual Guid? ExceptionalLog(Exception exception, Dictionary<string, string> customData, string categoryName, string appName)
        {
            return null;
        }
        public virtual string GetClientIp() => null;
        public virtual string GetCorrelationId() => null;
        public virtual string GetMethod() => null;
        public virtual string GetPage() => null;
        public virtual string GetPageRest() => null;
        public virtual string GetPageRest2() => null;
        public virtual string GetServerName() => null;
        public virtual string GetUri() => null;
        public virtual string GetUserAgent() => null;
        public virtual string TryGetPayload() => null;
    }
}
