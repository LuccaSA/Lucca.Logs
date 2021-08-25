using System;
using CloudNative.CloudEvents;

namespace Lucca.Logs.Shared
{
    public class LuccaLoggerOptions
    {
        /// <summary>
        /// Default application name
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Exceptional Connexion String
        /// </summary>
        public string ConnectionString { get; set; }

        public string ExceptionTableName { get; set; } = "Exceptions";

        /// <summary>
        /// Custom log file path
        /// </summary>
        public string LogFilePath { get; set; }

        /// <summary>
        /// Base URI for external link 
        /// </summary>
        /// <example>"http://opserver.com/exceptions/detail?id={0}"</example>
        public string GuidLink { get; set; } = "http://opserver.com/exceptions/detail?id={0}";

        public bool GuidWithPlaceHolder
        {
            get
            {
                _guidWithPlaceHolder ??= GuidLink.Contains("{0}");
                return _guidWithPlaceHolder.Value;
            }
        }
 
        internal Func<CloudEvent> CloudEventAccessor { get; set; }
         
        private bool? _guidWithPlaceHolder;
    }
}