using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Lucca.Logs.Shared;

namespace Lucca.Logs.AspnetLegacy
{
    public class ExceptionHandler : IExceptionHandler
    {
        private void Handle(ExceptionHandlerContext context)
        {
            IExceptionQualifier? efilter = context.RequestContext.Configuration.DependencyResolver.GetService(typeof(IExceptionQualifier)) as IExceptionQualifier;
            if (efilter is null)
            {
                return;
            }

            string message = efilter.DisplayExceptionDetails(context.Exception) ? context.Exception.Message : efilter.GenericErrorMessage;

            context.Result = new TextPlainErrorResult(context.ExceptionContext.Request, message);
        }

        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            Handle(context);
            return Task.CompletedTask;
        }
         
        private class TextPlainErrorResult : IHttpActionResult
        {
            public HttpRequestMessage Request { get; }
            public string Content { get; }

            public TextPlainErrorResult(HttpRequestMessage request, string content)
            {
                Request = request;
                Content = content;
            }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.Content = new StringContent(Content);
                response.RequestMessage = Request;
                return Task.FromResult(response);
            }
        }
    }
}