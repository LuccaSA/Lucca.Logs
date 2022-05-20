using System;
using System.Net;

namespace Lucca.Logs.Shared
{
    /// <summary>
    /// Filter exceptions for exceptional storage
    /// </summary>
    public interface IExceptionQualifier
    {
        /// <summary>
        /// If returns true, the exception will be propagated to OpServer (via exceptional)
        /// </summary>
        bool LogToOpserver(Exception exception);

        /// <summary>
        /// Returns true if exceptions details can not be propagated publicly
        /// </summary>
        bool DisplayExceptionDetails(Exception exception);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        HttpStatusCode? StatusCode(Exception exception);

        /// <summary>
        /// Default error message, aimed to avoid leaking exceptions details
        /// </summary>
        string GenericErrorMessage { get; }

        /// <summary>
        /// Prefered response content type
        /// </summary>
        string PreferedResponseType(string path);
    }

}