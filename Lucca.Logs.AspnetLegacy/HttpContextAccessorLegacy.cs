using System.Web;

namespace Lucca.Logs.AspnetLegacy
{
    public sealed class HttpContextAccessorLegacy : IHttpContextAccessor
    {
        public HttpContext HttpContext => HttpContext.Current;
    }
}