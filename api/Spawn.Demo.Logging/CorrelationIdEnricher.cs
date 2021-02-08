using Serilog.Core;
using Serilog.Events;

namespace Spawn.Demo.Logging
{
    public class CorrelationIdEnricher : ILogEventEnricher
    {
        internal const string Key = "RequestId";
        private readonly string _correlationId;

        public CorrelationIdEnricher(string correlationId)
        {
            _correlationId = correlationId;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(Key, _correlationId, true));
        }
    }
}