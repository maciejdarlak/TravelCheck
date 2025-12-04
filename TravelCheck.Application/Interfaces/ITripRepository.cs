using TravelCheck.Domain.Entities;

namespace TravelCheck.Application.Interfaces;

public interface ITripRepository
{
    Task AddAsync(Trip trip);
    Task<IEnumerable<Trip>> GetAllAsync();
    Task<Trip?> GetByIdAsync(Guid id);
    Task UpdateAsync(Trip trip);
    Task DeleteAsync(Guid id);
}
