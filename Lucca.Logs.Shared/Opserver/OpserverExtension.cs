using System;

namespace Lucca.Logs.Shared.Exceptional
{
    public static class OpserverExtension
    {
        /// <summary>
        /// Returns if the type of the exception is built into the .NET framework.
        /// </summary>
        /// <param name="e">The exception to check.</param>
        /// <returns>True if the exception is a type from within the CLR, false if it's a user/third party type.</returns>
        public static bool IsBCLException(this Exception e) => e.GetType().Module.ScopeName == "CommonLanguageRuntimeLibrary";

    }
}