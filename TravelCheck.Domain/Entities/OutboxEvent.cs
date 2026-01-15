namespace TravelCheck.Domain.Entities
{
    public class OutboxEvent
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public DateTimeOffset OccurredAt { get; set; }
        public bool Processed { get; set; }

        public string? CorrelationId { get; set; } // business correlation ID shared by all events of the same trip

        public string? TraceParent { get; set; }   // allows tracking one business flow across services (HTTP -> consumer)
        public string? TraceState { get; set; }    // additional notes to TraceParent
    }
}
