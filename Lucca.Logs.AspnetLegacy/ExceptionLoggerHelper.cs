using System;
using System.Linq;
using System.Web.Http.Controllers;

namespace Lucca.Logs.AspnetLegacy
{
    public static class ExceptionLoggerHelper
    {
        /// <summary>
        /// Helps logging an excpetions in an HttpContext
        /// </summary>
        public static void HttpLog(this Exception exception, HttpControllerContext context)
        {
            Logger.LogException(exception, context.TryGetAppName());
        }

        private static string TryGetAppName(this HttpControllerContext controllerContext)
        {
            if (controllerContext?.ControllerDescriptor?.ControllerType == null)
                return null;
            string appNameSpace = controllerContext.ControllerDescriptor.ControllerType.Namespace;
            if (appNameSpace != null)
            {
                string[] nsChunks = appNameSpace.Split('.');
                if (nsChunks.Length > 0)
                    return nsChunks.FirstOrDefault();
            }
            return null;
        }
    }
}