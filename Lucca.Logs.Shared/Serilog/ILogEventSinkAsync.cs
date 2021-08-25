using Serilog.Events;
using System.Threading.Tasks;

namespace Lucca.Logs.Shared
{
    public interface ILogEventSinkAsync
    {
        /// <summary>Emit the provided log event to the sink.</summary>
        /// <param name="logEvent">The log event to write.</param>
        Task EmitAsync(LogEvent logEvent);
    }
}