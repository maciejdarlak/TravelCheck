namespace TravelCheck.Application.Interfaces;

public interface IProcessedMessageRepository
{
    Task<bool> ExistsAsync(string messageId, CancellationToken ct = default);
    Task AddAsync(string messageId, CancellationToken ct = default);
}
