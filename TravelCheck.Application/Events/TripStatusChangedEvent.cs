using TravelCheck.Domain.Enums;

namespace TravelCheck.Application.Events;

public record TripStatusChangedEvent(Guid TripId, TripStatus NewStatus);
