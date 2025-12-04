namespace TravelCheck.Application.Dtos;

// data for creating new trip
public record CreateTripDto(
    string EmployeeName,
    string Country,
    DateTime From,
    DateTime To
);
