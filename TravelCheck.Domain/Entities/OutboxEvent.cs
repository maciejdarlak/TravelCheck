namespace TravelCheck.Domain.Entities
{
    public class OutboxEvent
    {
        public Guid Id { get; set; }

        // Partition key (e.g. "TripCreatedEvent")
        public string Type { get; set; } = null!;

        // Serialized event payload
        public string Payload { get; set; } = null!;

        // UTC timestamp of event creation
        public DateTimeOffset OccurredAt { get; set; }

        // Whether the event was already published to Service Bus
        public bool Processed { get; set; }
    }
}
