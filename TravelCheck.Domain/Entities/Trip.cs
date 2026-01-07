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
    public string? RejectionReason { get; private set; } 

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
    public void Reject(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            Status = TripStatus.Rejected;
            RejectionReason = reason;
        }
    }

    public void ChangeStatus(TripStatus newStatus)
    {
        if (Status == TripStatus.Rejected || Status == TripStatus.Completed)
            throw new InvalidOperationException("Cannot change status when trip is finalized.");

        if (Status == TripStatus.New && newStatus != TripStatus.Processing)
            throw new InvalidOperationException("New trip can only go to Processing");

        if (Status == TripStatus.Processing &&
            newStatus != TripStatus.Completed &&
            newStatus != TripStatus.Rejected)
            throw new InvalidOperationException("Processing trip can only go to Completed or Rejected");

        Status = newStatus;
    }
}
