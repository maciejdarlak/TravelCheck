using TravelCheck.Domain.Enums;

namespace TravelCheck.Application.Dtos;

// data returned to client
public record TripDto(
    Guid Id,
    string EmployeeName,
    string Country,
    DateTime From,
    DateTime To,
    TripStatus Status
);
