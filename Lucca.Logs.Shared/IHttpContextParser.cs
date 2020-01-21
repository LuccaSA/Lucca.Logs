using System;
using System.Collections.Generic;

namespace Lucca.Logs.Shared
{
    public interface IHttpContextParser
    {
        bool IsContextDefined { get; }
        string ExtractUrl(Uripart uriPart);
        string Method { get; }
        string Ip { get; }
        bool ContainsHeader(string header);
        string GetHeader(string header);
        string TryGetBodyContent();

        Guid? ExceptionalLog(Exception exception, Dictionary<string, string> customData, string categoryName, string appName);
    }
}