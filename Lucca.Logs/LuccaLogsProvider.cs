using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog; 

namespace Lucca.Logs
{
	public sealed class LuccaLogsProvider : ILoggerProvider
	{
		private readonly LuccaLoggerOptions _options;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public LuccaLogsProvider(IOptions<LuccaLoggerOptions> options, IHttpContextAccessor httpContextAccessor)
		{
			_options = options.Value;
			_httpContextAccessor = httpContextAccessor;

			LogManager.Configuration = _options.Nlog; 
        }

		public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
            => new LuccaLogger(categoryName, _httpContextAccessor, LogManager.GetLogger(categoryName), _options, string.Empty);

        public void Dispose()
        {
            // no disposable ressources
        }
    }
}