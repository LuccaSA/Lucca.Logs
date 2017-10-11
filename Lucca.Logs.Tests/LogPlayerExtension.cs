using System;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Lucca.Logs.Tests
{
	public static class LogPlayerExtension
	{
		public static void Log<T>(this ILogPlayer<T> player, LogLevel logLevel, int eventId, Exception exception, string message)
			where T : class 
		{
			switch (logLevel)
			{
				case LogLevel.Trace:
					player.Logger.LogTrace(eventId, exception, message);
					break;
				case LogLevel.Debug:
					player.Logger.LogDebug(eventId, exception, message);
					break;
				case LogLevel.Information:
					player.Logger.LogInformation(eventId, exception, message);
					break;
				case LogLevel.Warning:
					player.Logger.LogWarning(eventId, exception, message);
					break;
				case LogLevel.Error:
					player.Logger.LogError(eventId, exception, message);
					break;
				case LogLevel.Critical:
					player.Logger.LogCritical(eventId, exception, message);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
			}
		}
	}
}
