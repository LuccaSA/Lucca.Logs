using System;
using System.Collections.Generic;

namespace Lucca.Logs.Abstractions
{
    public interface IHttpContextParser
    {
        bool IsContextDefined { get; }
        string ExtractUrl(UriParts uriPart);
        string Method { get; }
        string Ip { get; }
        bool ContainsHeader(string header);
        string GetHeader(string header);
        string TryGetBodyContent();

        Guid? ExceptionalLog(Exception exception, Dictionary<string, string> customData, string categoryName, string appName);
    }
}