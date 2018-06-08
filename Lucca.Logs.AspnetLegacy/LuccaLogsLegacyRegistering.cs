using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;

namespace Lucca.Logs.AspnetLegacy
{
    public static class LuccaLogsLegacyRegistering
    {
        public static void AddLuccaLogs(this ServicesContainer servicesContainer)
        {
            servicesContainer.Add(typeof(IExceptionLogger), new LuccaExceptionLogger());
            servicesContainer.Replace(typeof(IExceptionHandler), new LuccaExceptionHandler());
        }
    }
}