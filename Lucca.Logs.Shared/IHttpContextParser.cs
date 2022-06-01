using System;
using System.Collections.Generic;
using System.Net;

namespace Lucca.Logs.Shared
{
    public interface IHttpContextParser
    {
        Guid? ExceptionalLog(Exception? exception, Dictionary<string, string?> customData, string categoryName, string appName);
        IHttpContextRequest? HttpRequestAccessor();
    }

    public interface IHttpContextRequest
    {
        string ExtractUrl(Uriparts uriPart);
        string? GetHeader(string header);
        string? TryGetBodyContent();
        string GetMethod();
        IPAddress? HostAddress();
    }
}