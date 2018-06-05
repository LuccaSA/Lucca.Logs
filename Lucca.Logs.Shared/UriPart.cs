using System;

namespace Lucca.Logs.Shared
{
    [Flags]
    public enum UriPart
    {
        None = 0,
        Scheme = 1,
        Host = 1 << 1,
        Port = 1 << 2,
        Path = 1 << 3,
        Query = 1 << 4,
        Full = Scheme | Host | Port | Path | Query
    }
}