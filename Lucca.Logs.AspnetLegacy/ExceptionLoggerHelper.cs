using System;
using System.Linq;
using System.Web.Http.Controllers;

namespace Lucca.Logs.AspnetLegacy
{
    public static class ExceptionLoggerHelper
    {
        public const string DefaultAppName = "UnknownAppName";

        /// <summary>
        /// Helps logging an exception in an HttpContext
        /// </summary>
        public static void HttpLog(this Exception exception, HttpControllerContext? context)
        {
            Logger.LogException(exception, context.TryGetAppName() ?? DefaultAppName);
        }

        private static string? TryGetAppName(this HttpControllerContext? controllerContext)
        {
            if (controllerContext?.ControllerDescriptor?.ControllerType is null)
            {
                return null;
            }

            string appNameSpace = controllerContext.ControllerDescriptor.ControllerType.Namespace;
            if (appNameSpace is not null)
            {
                string[] nsChunks = appNameSpace.Split('.');
                if (nsChunks.Length > 0)
                {
                    return nsChunks.FirstOrDefault();
                }
            }
            return null;
        }
    }
}