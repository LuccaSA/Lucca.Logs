﻿using System;
using System.Collections.Generic;

namespace Lucca.Logs.Shared
{
    public interface IHttpContextWrapper
    {
        string ExtractUrl(UriPart uriPart);
        string Method { get; }
        string Ip { get; }
        bool ContainsHeader(string header);
        string GetHEader(string header);
        string TryGetBodyContent();

        Guid? ExceptionalLog(Exception exception, Dictionary<string, string> customData, string categoryName, string appName);
    }
}