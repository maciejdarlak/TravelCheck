using TravelCheck.Application.Interfaces;
using TravelCheck.Application.Dtos;
using TravelCheck.Domain.Entities;
using TravelCheck.Domain.Enums;

namespace TravelCheck.Application.Services;

public class TripService
{
    private readonly ITripRepository _repository;

    public TripService(ITripRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> CreateAsync(CreateTripDto dto)
    {
        var trip = new Trip(dto.EmployeeName, dto.Country, dto.From, dto.To); // mapping

        await _repository.AddAsync(trip);

        return trip.Id;
    }

    public async Task<IEnumerable<Trip>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Trip?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Trip> UpdateAsync(Guid id, UpdateTripDto dto)
    {
        var trip = await _repository.GetByIdAsync(id);

        if (trip == null)
            throw new Exception("trip not found");

        trip.UpdateDetails(dto.EmployeeName, dto.Country, dto.From, dto.To); // mapping

        await _repository.UpdateAsync(trip);   

        return trip;
    }

    public async Task<Trip> DeleteAsync(Guid id)
    {
        var trip = await _repository.GetByIdAsync(id);

        if (trip == null)
            throw new Exception("trip not found");

        await _repository.DeleteAsync(id);

        return trip;
    }

    public async Task<Trip> ChangeStatusAsync(Guid id, TripStatus newStatus)
    {
        var trip = await _repository.GetByIdAsync(id);

        if (trip == null)
            throw new Exception("trip not found");

        trip.ChangeStatus(newStatus);

        await _repository.UpdateAsync(trip);

        return trip;
    }
}