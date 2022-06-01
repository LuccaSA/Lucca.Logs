using Microsoft.Extensions.Logging;

namespace Lucca.Logs.Netcore.Tests
{
	public interface ILogPlayer<T>
		where T : class
	{
		ILogger<T> Logger { get; }
	}
}