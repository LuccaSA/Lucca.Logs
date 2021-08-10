using System;
using System.Text.RegularExpressions;

namespace Lucca.Logs.Shared
{

    internal static class CleanExtension
    {
        private static readonly Regex _passwordClean = new Regex("(?<=[?&]" + Regex.Escape("password") + "=)[^&]*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string ClearQueryStringPassword(this string source)
        {
            if (source == null)
            {
                return null;
            }

            if (source.IndexOf("password", StringComparison.OrdinalIgnoreCase) > -1)
            {
                return _passwordClean.Replace(source, "***");
            }

            return source;
        }
    }
}
