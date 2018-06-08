using System;

namespace Lucca.Logs.Shared
{
    /// <summary>
    /// Filter exceptions for exceptional storage
    /// </summary>
    public interface IExceptionalFilter
    {
        /// <summary>
        /// If returns true, the exception will not be propagated to exceptional
        /// </summary>
        bool FilterException(Exception exception);

        /// <summary>
        /// Returns true if this is an internal exception.
        /// Such exceptions details will not be propagated publicly
        /// </summary>
        bool IsInternalException(Exception exception);

        /// <summary>
        /// Default error message, aimed to avoid leaking exceptions details
        /// </summary>
        string GenericErrorMessage { get; }
    }
}