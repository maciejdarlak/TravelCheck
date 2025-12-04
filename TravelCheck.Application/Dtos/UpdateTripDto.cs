namespace TravelCheck.Application.Dtos;

// data for creating new trip
public record UpdateTripDto(
    string EmployeeName,
    string Country,
    DateTime From,
    DateTime To
);
