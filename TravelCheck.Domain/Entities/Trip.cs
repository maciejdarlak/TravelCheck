using TravelCheck.Domain.Enums;

namespace TravelCheck.Domain.Entities;

public class Trip
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string EmployeeName { get; private set; }
    public string Country { get; private set; }
    public DateTime From { get; private set; }
    public DateTime To { get; private set; }
    public TripStatus Status { get; private set; }

    public Trip(string employeeName, string country, DateTime from, DateTime to)
    {
        EmployeeName = employeeName;
        Country = country;
        From = from;
        To = to;
        Status = TripStatus.New;
    }

    public void UpdateDetails(string employeeName, string country, DateTime from, DateTime to)
    {
        if (string.IsNullOrWhiteSpace(employeeName))
            throw new ArgumentException("employee name is required");

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("country is required");

        if (from > to)
            throw new ArgumentException("from date cannot be after to date");

        EmployeeName = employeeName;
        Country = country;
        From = from;
        To = to;
    }

    public void MarkProcessing() => Status = TripStatus.Processing;
    public void MarkCompleted() => Status = TripStatus.Completed;
    public void Reject() => Status = TripStatus.Rejected;
}
