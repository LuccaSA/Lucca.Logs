using System.Web;

namespace Lucca.Logs.AspnetLegacy
{
    public interface IHttpContextAccessor
    {
        HttpContext HttpContext { get; }
    }
}