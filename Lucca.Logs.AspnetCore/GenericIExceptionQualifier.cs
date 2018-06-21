using System;
using System.Net;
using System.Net.Http;
using Lucca.Logs.Shared;

namespace Lucca.Logs.AspnetCore
{
    public class GenericExceptionQualifier : IExceptionQualifier
    {
        public virtual bool LogToOpserver(Exception exception)
        {
            return true;
        }

        public virtual bool DisplayExceptionDetails(Exception exception)
        {
            return false;
        }

        public virtual HttpStatusCode? StatusCode(Exception exception)
        {
            switch (exception)
            {
                case UnauthorizedAccessException _:
                    return HttpStatusCode.Unauthorized;
                case HttpRequestException _:
                    return HttpStatusCode.InternalServerError;
            }
            return null;
        }

        public virtual string GenericErrorMessage => "Oops ! Something went wrong. Please contact your administrator";

        public virtual string PreferedResponseType => "application/json";
    }
}