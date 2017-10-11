using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Lucca.Logs
{
	public class LuccaLogsProvider : ILoggerProvider
	{
		private readonly LuccaLoggerOptions _options;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public LuccaLogsProvider(LuccaLoggerOptions options, IHttpContextAccessor httpContextAccessor)
		{
			_options = options;
			_httpContextAccessor = httpContextAccessor;
			LogManager.Configuration = options.NlogLoggingConfiguration;
		}

		public ILogger CreateLogger(string categoryName)
            => new LuccaLogger(categoryName, _httpContextAccessor, LogManager.GetLogger(categoryName), _options, string.Empty);

        public void Dispose()
        {
        }
    }
}