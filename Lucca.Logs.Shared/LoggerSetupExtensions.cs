using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System;
using System.IO;
using System.Text;

namespace Lucca.Logs.Shared
{
    public static class LoggerSetupExtensions
    {
        public static LoggerConfiguration CreateLoggerConfiguration(
            LuccaLoggerOptions options,  
            IHttpContextParser httpContextWrapper, 
            LogExtractor logExtractor, 
            IExceptionalWrapper exceptionalWrapper, 
            IExceptionQualifier filters)
        {
            exceptionalWrapper.Configure(exceptionalSetting =>
            {
                exceptionalSetting.DefaultStore = options.GenerateExceptionalStore();

                exceptionalSetting.LogFilters.Cookie["password"] = "***";
                exceptionalSetting.LogFilters.Header["password"] = "***";
                exceptionalSetting.LogFilters.Form["password"] = "***";
                exceptionalSetting.LogFilters.Header["password"] = "***";
                exceptionalSetting.LogFilters.QueryString["password"] = "***";

                exceptionalSetting.OnBeforeLog += (o, eb) =>
                {
                    var querystring = eb?.Error?.ServerVariables?.Get("QUERY_STRING");
                    if (querystring != null)
                    {
                        eb.Error.ServerVariables.Set("QUERY_STRING", querystring.ClearQueryStringPassword());
                    }
                };
            });

            string path = PrepareLogPath(options);

            var loggerConf = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.With(new LuccaLogsEnricher(options, httpContextWrapper, logExtractor, exceptionalWrapper, filters))
                .WriteTo.Async(conf =>
                { 
                    conf.File(new JsonFormatter(), path, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 31, encoding: Encoding.UTF8);
                    conf.Console();
                }).MinimumLevel.Is(LogEventLevel.Debug);

            if (options.TestSink != null)
                loggerConf = loggerConf.WriteTo.Sink(options.TestSink);

            return loggerConf;
        }

        private static string PrepareLogPath(LuccaLoggerOptions options)
        {
            string path;
            if (Path.IsPathRooted(options.LogFilePath))
            {
                path = options.LogFilePath;
            }
            else if (!string.IsNullOrWhiteSpace(options.LogFilePath))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, options.LogFilePath);
            }
            else
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "/logs/logfile.txt");
            }

            return path;
        }
    }
}