using System;
using System.Collections.Generic;

namespace Lucca.Logs.Shared
{
    public interface IHttpContextParser
    {
        string ExtractUrl(Uripart uriPart, IHttpContextRequest httpRequest);
        bool ContainsHeader(string header, IHttpContextRequest httpRequest);
        string GetHeader(string header, IHttpContextRequest httpRequest);
        string TryGetBodyContent(IHttpContextRequest httpRequest);
        Guid? ExceptionalLog(Exception exception, Dictionary<string, string> customData, string categoryName, string appName);
        IHttpContextRequest HttpRequestAccessor();
        string GetMethod(IHttpContextRequest httpRequest);
        string HostAddress(IHttpContextRequest httpRequest);
    }

    public interface IHttpContextRequest
    {
    }
}