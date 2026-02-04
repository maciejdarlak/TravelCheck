using TravelCheck.Application.Interfaces;
using TravelCheck.Application.Dtos;
using TravelCheck.Domain.Entities;
using TravelCheck.Domain.Enums;
using TravelCheck.Application.Events;
using TravelCheck.Application.Outbox;

namespace TravelCheck.Application.Services;

public class TripService
{
    private readonly ITripRepository _repository;
    private readonly IOutboxEventPublisher _outboxPublisher;

    public TripService(ITripRepository repository, IOutboxEventPublisher outboxPublisher)
    {
        _repository = repository;
        _outboxPublisher = outboxPublisher;
    }

    // CREATE → TripCreatedEvent
    public async Task<Guid> CreateAsync(CreateTripDto dto)
    {
        var trip = new Trip(dto.EmployeeName, dto.Country, dto.From, dto.To);

        await _repository.AddAsync(trip);

        await _outboxPublisher.PublishAsync(new TripCreatedEvent(trip.Id));

        return trip.Id;
    }

    // READ 
    public async Task<IEnumerable<Trip>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Trip?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    // UPDATE → TripUpdatedEvent
    public async Task<Trip> UpdateAsync(Guid id, UpdateTripDto dto)
    {
        var trip = await _repository.GetByIdAsync(id);

        if (trip == null)
            throw new Exception("trip not found");

        trip.UpdateDetails(dto.EmployeeName, dto.Country, dto.From, dto.To);

        await _repository.UpdateAsync(trip);

        await _outboxPublisher.PublishAsync(new TripUpdatedEvent(trip.Id));

        return trip;
    }

    // DELETE → TripDeletedEvent
    public async Task<Trip> DeleteAsync(Guid id)
    {
        var trip = await _repository.GetByIdAsync(id);

        if (trip == null)
            throw new Exception("trip not found");

        await _repository.DeleteAsync(id);

        await _outboxPublisher.PublishAsync(new TripDeletedEvent(id));

        return trip;
    }

    // STATUS CHANGE → TripStatusChangedEvent (for WORKER - consumer only)
    public async Task<Trip> ChangeStatusAsync(Guid id, TripStatus newStatus)
    {
        var trip = await _repository.GetByIdAsync(id);

        if (trip == null)
            throw new Exception("trip not found");

        trip.ChangeStatus(newStatus);

        await _repository.UpdateAsync(trip);

        await _outboxPublisher.PublishAsync(
            new TripStatusChangedEvent(trip.Id, newStatus)
        );

        return trip;
    }
}
