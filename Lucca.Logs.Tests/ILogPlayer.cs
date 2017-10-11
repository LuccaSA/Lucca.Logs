using Microsoft.Extensions.Logging;

namespace Lucca.Logs.Tests
{
	public interface ILogPlayer<T>
		where T : class
	{
		ILogger<T> Logger { get; }
	}
}