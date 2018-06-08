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
    }
}