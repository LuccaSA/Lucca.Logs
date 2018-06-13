using System;
using Lucca.Logs.Shared;

namespace Lucca.Logs.AspnetCore
{
    public class GenericExceptionQualifier : IExceptionQualifier
    {
        public bool LogToOpserver(Exception exception)
        {
            return true;
        }

        public bool DisplayExceptionDetails(Exception exception)
        {
            return false;
        }

        public string GenericErrorMessage => "Oops ! Something went wrong. Please contact your administrator";
    }
}