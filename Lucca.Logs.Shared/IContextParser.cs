using System;
using System.Collections.Generic;

namespace Lucca.Logs.Shared
{
    public interface IContextParser
    {
        bool CanRead();
        string GetPageRest();
        string GetPageRest2();
        string GetPage();
        string GetMethod();
        string GetUri();
        string GetServerName();
        string GetClientIp();
        string GetCorrelationId();
        string GetUserAgent();
        string TryGetPayload();
        Guid? ExceptionalLog(Exception exception, Dictionary<string, string> customData, string categoryName, string appName);

    }
}