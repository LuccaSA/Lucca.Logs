using System;
using System.Net;
using System.Net.Http;
using Lucca.Logs.Shared;

namespace Lucca.Logs.AspnetCore
{
    public class GenericExceptionQualifier : IExceptionQualifier
    {
        internal static readonly string DefaultErrorMessage = "Oops ! Something went wrong. Please contact your administrator";
        public virtual bool LogToOpserver(Exception exception)
        {
            var statusCode = StatusCode(exception);
            return !statusCode.HasValue || statusCode.Value >= HttpStatusCode.InternalServerError;
        }

        public virtual bool DisplayExceptionDetails(Exception exception) => false;

        public virtual HttpStatusCode? StatusCode(Exception exception) => exception switch
        {
            UnauthorizedAccessException _ => HttpStatusCode.Unauthorized,
            HttpRequestException _ => HttpStatusCode.InternalServerError,
            _ => null,
        };

        public virtual string GenericErrorMessage => DefaultErrorMessage;

        public virtual string PreferedResponseType(string path) => "application/json";
    }
}