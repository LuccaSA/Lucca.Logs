using System;
using System.Text;
using NLog;
using NLog.Config;
using NLog.Targets;
using StackExchange.Exceptional;
using StackExchange.Exceptional.Stores;

namespace Lucca.Logs
{
    public class LuccaLoggerOptions
    {
        public LuccaLoggerOptions()
        {
            EventIdSeparator = ".";
        }

        /// <summary>
        /// Default application name
        /// </summary>
        public string AplicationName { get; set; }

        /// <summary>
        /// Exceptional Connexion String
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Custom log file path
        /// </summary>
        public string LogFilePath { get; set; }

        /// <summary>
        /// Optional network Api URI for direct logging
        /// </summary>
        /// <example>""https://api.logmatic.io/v1/input/myLogmaticKey"</example>
        public string ApiUri { get; set; }

        /// <summary>
        /// Base URI for external link 
        /// </summary>
        /// <example>"http://opserver.lucca.local/exceptions/detail?id="</example>
        public string GuidLink { get; set; }

        /// <summary>
        /// Separator between for EventId.Id and EventId.Name. Default to .
        /// </summary>
        public string EventIdSeparator { get; set; }
        /// <summary>
        /// Skip allocation of <see cref="LogEventInfo.Properties" />-dictionary
        /// </summary>
        /// <remarks>
        /// using
        ///     <c>default(EventId)</c></remarks>
        public bool IgnoreEmptyEventId { get; set; }

        public LoggingConfiguration Nlog
        {
            get => _nlog ?? (_nlog = GenerateLuccaDefaultConfig());
            set => _nlog = value;
        }

        public ExceptionalSettings Exceptional
        {
            get => _exceptional ?? (_exceptional = GenerateLuccaDefaultErrorStore());
            set => _exceptional = value;
        }

        private LoggingConfiguration _nlog;
        private ExceptionalSettings _exceptional;

        private ExceptionalSettings GenerateLuccaDefaultErrorStore()
        {
            ErrorStore errorStore;

            if (!String.IsNullOrEmpty(ConnectionString))
            {
                errorStore = new SQLErrorStore(ConnectionString, AplicationName);
            }
            else
            {
                errorStore = new MemoryErrorStore(new ErrorStoreSettings { ApplicationName = AplicationName });
            }

            var setting = new ExceptionalSettings();

            setting.DefaultStore = errorStore;

            return setting;
        }

        private LoggingConfiguration GenerateLuccaDefaultConfig()
        {
            var nLogConfig = new LoggingConfiguration();

            if (!string.IsNullOrEmpty(ApiUri))
            {
                // NetworkTarget : pour envoyer sur Logmatic
                var networkTarget = new NetworkTarget("logmatic")
                {
                    Address = ApiUri,
                    Layout = LuccaDataWrapper.GenerateJsonLayout()
                };
                var networkRule = new LoggingRule("*", LogLevel.Trace, networkTarget);
                nLogConfig.LoggingRules.Add(networkRule);
            }
            else
            {

                // FileTarget : pour stoquer localement les excpetions
                var fileTarget = new FileTarget("localTarget")
                {
                    FileName = LogFilePath ?? "${basedir}/logs/logfile.txt",
                    Encoding = Encoding.UTF8,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveFileKind = FilePathKind.Relative,
                    ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                    LineEnding = LineEndingMode.CRLF,
                    ArchiveAboveSize = 1048576,
                    MaxArchiveFiles = 100,
                    Layout = LuccaDataWrapper.GenerateJsonLayout()
                };
                // 1Mb file size
                var fileRule = new LoggingRule("*", LogLevel.Trace, fileTarget);
                nLogConfig.LoggingRules.Add(fileRule);
            }

            return nLogConfig;
        }

    }
}