using System;
using System.Text;
using CloudNative.CloudEvents;
using NLog;
using NLog.Config;
using NLog.Targets;
using StackExchange.Exceptional;
using StackExchange.Exceptional.Stores;

namespace Lucca.Logs.Shared
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
        public string? ApplicationName { get; set; }

        /// <summary>
        /// Exceptional Connexion String
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Custom log file path
        /// </summary>
        public string? LogFilePath { get; set; }

        /// <summary>
        /// Base URI for external link 
        /// </summary>
        /// <example>"http://opserver.lucca.local/exceptions/detail?id={0}"</example>
        public string GuidLink { get; set; } = "http://opserver.lucca.local/exceptions/detail?id={0}";

        private bool? _guidWithPlaceHolder;
        public bool GuidWithPlaceHolder
        {
            get
            {
                if (!_guidWithPlaceHolder.HasValue)
                {
                    _guidWithPlaceHolder = GuidLink.Contains("{0}");
                }
                return _guidWithPlaceHolder.Value;
            }
        }
        
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

        internal Func<CloudEvent>? CloudEventAccessor { get; set; }

        private LoggingConfiguration? _nlog;
        public LoggingConfiguration Nlog
        {
            get => _nlog ??= GenerateLuccaDefaultConfig();
            set => _nlog = value;
        }

        internal ErrorStore? ExplicitErrorStore { get; set; }

        public ErrorStore GenerateExceptionalStore()
        {
            if (!string.IsNullOrEmpty(ConnectionString))
            {
                return new SQLErrorStore(ConnectionString, ApplicationName!);
            }

            if (ExplicitErrorStore is not null)
            {
                return ExplicitErrorStore;
            }

            return new MemoryErrorStore(new ErrorStoreSettings { ApplicationName = ApplicationName! });
        }

        private LoggingConfiguration GenerateLuccaDefaultConfig()
        {
            var nLogConfig = new LoggingConfiguration();

            // FileTarget : to save exceptions locally
            var fileTarget = new FileTarget("localTarget")
            {
                FileName = LogFilePath ?? "${basedir}/logs/logfile.txt",
                Encoding = Encoding.UTF8,
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveFileKind = FilePathKind.Relative,
                ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                EnableArchiveFileCompression = true,
                ArchiveOldFileOnStartup = true,
                LineEnding = LineEndingMode.CRLF,
                ArchiveAboveSize = 1048576,
                MaxArchiveFiles = 100,
                Layout = LogMeta.LuccaJsonLayout
            };

            // 1Mb file size
            var fileRule = new LoggingRule("*", LogLevel.Trace, fileTarget);
            nLogConfig.LoggingRules.Add(fileRule);

            return nLogConfig;
        }
    }
}