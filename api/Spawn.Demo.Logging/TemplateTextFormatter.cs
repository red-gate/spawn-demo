using System;
using System.IO;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace Spawn.Demo.Logging
{
    public class TemplateTextFormatter : ITextFormatter
    {
        private const string TimestampTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss}";
        private const string BodyTemplate = "{Message}{NewLine}{Exception}";
        private const string LevelTemplate = "[{Level}]";

        private static readonly string CorrelationIdTemplate = $"[Correlation Id: {{{CorrelationIdEnricher.Key}}}]";

        private static readonly string DefaultConsoleOutputTemplate =
            $"{TimestampTemplate} {LevelTemplate} {BodyTemplate}";

        private static readonly string CorrelationIdOutputTemplate =
            $"{TimestampTemplate} {LevelTemplate} {CorrelationIdTemplate} {BodyTemplate}";

        private readonly ITextFormatter _baseFormatter;
        private readonly ITextFormatter _correlationIdFormatter;

        public TemplateTextFormatter(IFormatProvider provider = null)
        {
            _baseFormatter = new MessageTemplateTextFormatter(DefaultConsoleOutputTemplate, provider);
            _correlationIdFormatter = new MessageTemplateTextFormatter(CorrelationIdOutputTemplate, provider);
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            GetFormatterForEvent(logEvent).Format(logEvent, output);
        }

        private ITextFormatter GetFormatterForEvent(LogEvent logEvent)
        {
            var correlationId = logEvent.Properties.ContainsKey(CorrelationIdEnricher.Key);

            if (correlationId)
            {
                return _correlationIdFormatter;
            }

            return _baseFormatter;
        }
    }
}