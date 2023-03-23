using System;
using CloudNative.CloudEvents;
using Lucca.Logs.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using StackExchange.Exceptional;
using Serilog.Core;
using Microsoft.Extensions.Options;
using Serilog;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Net;
#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.Http;
#else
using Lucca.Logs.AspnetLegacy;
#endif

namespace Lucca.Logs.AspnetCore
{
    public static class LuccaLoggerExtensions
    {
        public static IServiceCollection WithCloudEvents(this IServiceCollection services, Func<CloudEvent> cloudEventAccessor)
        {
            services.PostConfigure<LuccaLoggerOptions>(o => o.CloudEventAccessor = cloudEventAccessor);
            services.AddSingleton<ILogDetailsExtractor, CloudEventExtractor>();
            return services;
        }

        public static IServiceCollection AddLuccaLogs(this IServiceCollection services, IConfigurationSection config, string appName, Action<LuccaLoggerOptions>? configureOptions = null)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (string.IsNullOrWhiteSpace(appName))
            {
                throw new ArgumentNullException(nameof(appName));
            }

            if (!config.Exists())
            {
                throw new LogConfigurationException("Missing configuration section");
            }

            services.AddOptions();
            services.Configure<LuccaLoggerOptions>(config);

#if NET6_0_OR_GREATER
            services.AddExceptional(e =>
            {
                var luccaLogsOption = config.Get<LuccaLoggerOptions>();
                e.DefaultStore = luccaLogsOption.GenerateExceptionalStore();
                e.DataIncludeRegex = new Regex("Lucca.*", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            });
#endif
            if (configureOptions is not null)
            {
                services.Configure(configureOptions);
            }

            services.PostConfigure<LuccaLoggerOptions>(o =>
            {
                if (string.IsNullOrWhiteSpace(o.ApplicationName))
                {
                    o.ApplicationName = appName;
                }
            });
            services.RegisterLuccaLogsProvider();
            return services;
        }

        public static IServiceCollection AddLuccaLogs(this IServiceCollection services, Action<LuccaLoggerOptions> configureOptions, string appName, ErrorStore? errorStore = null)
        {
            if (string.IsNullOrWhiteSpace(appName))
            {
                throw new ArgumentNullException(nameof(appName));
            }

            services.AddOptions();

            if (configureOptions is not null)
            {
                services.Configure<LuccaLoggerOptions>(o =>
                {
                    o.ExplicitErrorStore = errorStore;
                    configureOptions(o);
                });
            }
#if NET6_0_OR_GREATER
            if (errorStore is not null)
            {
                services.AddExceptional(o =>
                {
                    o.Store.Type = errorStore.GetType().ToString();
                    o.DefaultStore = errorStore;
                });
            }
#endif

            services.PostConfigure((Action<LuccaLoggerOptions>)(o =>
            {
                if (string.IsNullOrWhiteSpace(o.ApplicationName))
                {
                    o.ApplicationName = appName;
                }

#if !NET6_0_OR_GREATER
                if (errorStore is null)
                {
                    errorStore = o.GenerateExceptionalStore();
                }
                Exceptional.Configure(o =>
                {
                    o.Store.Type = errorStore.GetType().ToString();
                    o.DefaultStore = errorStore;
                });
#endif

                Exceptional.Configure(exceptionalSetting =>
                {
                    exceptionalSetting.DefaultStore = errorStore;

                    exceptionalSetting.LogFilters.Cookie["password"] = "***";
                    exceptionalSetting.LogFilters.Header["password"] = "***";
                    exceptionalSetting.LogFilters.Form["password"] = "***";
                    exceptionalSetting.LogFilters.Header["password"] = "***";
                    exceptionalSetting.LogFilters.QueryString["password"] = "***";

                    exceptionalSetting.OnBeforeLog += (o, eb) =>
                    {
                        string? querystring = eb?.Error?.ServerVariables?.Get("QUERY_STRING");
                        if (querystring is not null)
                        {
                            eb!.Error.ServerVariables.Set("QUERY_STRING", querystring.ClearQueryStringPassword());
                        }
                        
                        ClearGuids(eb?.Error?.Cookies);

                        ClearGuids(eb?.Error?.RequestHeaders);
                    };
                });

            }));

            services.RegisterLuccaLogsProvider();
            return services;
        }

        private static void ClearGuids(NameValueCollection? nc)
        {
            if (nc is null)
            {
                return;
            }
            foreach (string key in nc)
            {
                nc[key] = RedactContent(nc[key]);
            }
        }

        private static Regex _guidRegex = new Regex(@"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", RegexOptions.Compiled);

        private static string? RedactContent(string? value)
        {
            if (value is null)
            {
                return null;
            }
            var matches = _guidRegex.Matches(value);
            foreach (Match match in matches)
            {
                var guid = match.Value;
                var redactedGuid = RedactGuid(guid);
                value = value.Replace(guid, redactedGuid);
            }
            return value;
        }

        private static string RedactGuid(string guid)
        {
            var guidParts = guid.Split('-');
            var redactedGuid = $"{guidParts[0]}-{guidParts[1]}-xxxx-xxxx-xxxxxxxxxxxx";
            return redactedGuid;
        }

        private static void RegisterLuccaLogsProvider(this IServiceCollection services)
        {
            services.AddSingleton<LogExtractor>();
            services.AddSingleton<EnvironmentDetailsExtractor>();

            services.AddSingleton<ILogDetailsExtractor, HttpLogDetailsExtractor>();
            services.AddSingleton<ILogEventEnricher, LuccaLoggerEnricher>();

#if NET6_0_OR_GREATER
            services.TryAddSingleton<IExceptionQualifier, GenericExceptionQualifier>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<IHttpContextParser, HttpContextParserCore>();
            services.AddSingleton<IExceptionalWrapper, ExceptionalWrapperCore>();
#else
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessorLegacy>();
            services.TryAddSingleton<IHttpContextParser, HttpContextParserLegacy>();
            services.AddSingleton<IExceptionalWrapper, ExceptionalWrapperLegacy>();

            services.AddSingleton(sp =>
            {
                return LuccaLogsLegacyRegistering.BuildLogger(sp);
            });
#endif
        }
    }

}