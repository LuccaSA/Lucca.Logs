using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using System;
using System.IO;

namespace Lucca.Logs.Shared
{
    // Inspired by https://github.com/serilog/serilog-formatting-compact/blob/8393e0ab8c2bc746fc733a4f20731b9e1f20f811/src/Serilog.Formatting.Compact/Formatting/Compact/RenderedCompactJsonFormatter.cs
    public class LuccaJsonFormatter : ITextFormatter
    {
        readonly JsonValueFormatter _valueFormatter;

        public LuccaJsonFormatter(JsonValueFormatter? valueFormatter = null)
        {
            _valueFormatter = valueFormatter ?? new JsonValueFormatter(typeTagName: "$type");
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            FormatEvent(logEvent, output, _valueFormatter);
            output.WriteLine();
        }

        public static void FormatEvent(LogEvent logEvent, TextWriter output, JsonValueFormatter valueFormatter)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (valueFormatter == null) throw new ArgumentNullException(nameof(valueFormatter));

            output.Write("{\"date\":\"");
            output.Write(logEvent.Timestamp.UtcDateTime.ToString("O"));
            output.Write("\",\"message\":");
            var message = logEvent.MessageTemplate.Render(logEvent.Properties);
            JsonValueFormatter.WriteQuotedJsonString(message, output);

            output.Write(",\"hostname\":\"");
            output.Write(Environment.MachineName);
            output.Write('\"');

            if (logEvent.Level != LogEventLevel.Information)
            {
                output.Write(",\"level\":\"");
                output.Write(logEvent.Level);
                output.Write('\"');
            }

            if (logEvent.Exception != null)
            {
                output.Write(",\"exception\":");
                JsonValueFormatter.WriteQuotedJsonString(logEvent.Exception.ToString(), output);
            }

            foreach (var property in logEvent.Properties)
            {
                var name = property.Key;
                output.Write(',');
                JsonValueFormatter.WriteQuotedJsonString(name, output);
                output.Write(':');
                valueFormatter.Format(property.Value, output);
            }

            output.Write('}');
        }
    }
}