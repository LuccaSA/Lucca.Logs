using System.Text;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Lucca.Logs
{
	public class LuccaLoggerOptions
	{
		public LuccaLoggerOptions()
		{
			EventIdSeparator = ".";
		}

		public string ConnexionString { get; set; }
		public string LogFilePath { get; set; }
		public string LogmaticApiKey { get; set; }
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

        public LoggingConfiguration NlogLoggingConfiguration
        {
            get { return _nlogLoggingConfiguration = _nlogLoggingConfiguration ?? GenerateLuccaDefaultConfig(); }
            set => _nlogLoggingConfiguration = value;
        }

        /// <summary>
        /// Default options
        /// </summary>
        internal static LuccaLoggerOptions Default = new LuccaLoggerOptions();

        private LoggingConfiguration _nlogLoggingConfiguration;

        private LoggingConfiguration GenerateLuccaDefaultConfig()
        {
            var nLogConfig = new LoggingConfiguration();

            if (!string.IsNullOrEmpty(LogmaticApiKey))
            {
                // NetworkTarget : pour envoyer sur Logmatic
                var networkTarget = new NetworkTarget("logmatic")
                {
                    Address = "https://api.logmatic.io/v1/input/" + LogmaticApiKey,
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