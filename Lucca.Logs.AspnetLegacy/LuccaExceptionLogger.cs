using System;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;

namespace Lucca.Logs.AspnetLegacy
{
    public class LuccaExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            HttpControllerContext controllerContext = context.ExceptionContext?.ControllerContext;
            context.Exception.HttpLog(controllerContext);
        }
    }
}