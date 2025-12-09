using Microsoft.Azure.Cosmos;
using TravelCheck.Application.Interfaces;
using TravelCheck.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace TravelCheck.Infrastructure.Repositories;

public class CosmosTripRepository : ITripRepository
{
    private readonly Container _container;

    // CosmosClient → access to Cosmos DB
    // IConfiguration → access to appsettings.json
    public CosmosTripRepository(CosmosClient client, IConfiguration cfg)
    {
        var db = cfg["CosmosDb:DatabaseName"]!; 
        var containerName = cfg["CosmosDb:ContainerName"]!;
        _container = client.GetContainer(db, containerName);
    }

    public async Task AddAsync(Trip trip)
    {
        await _container.CreateItemAsync(trip, new PartitionKey(trip.Country));
    }

    public async Task<IEnumerable<Trip>> GetAllAsync()
    {
        var query = _container.GetItemQueryIterator<Trip>( // converts data into Trip objects
            new QueryDefinition("select * from c"));

        var results = new List<Trip>();

        while (query.HasMoreResults)
        {
            foreach (var item in await query.ReadNextAsync())
            {
                results.Add(item);
            }
        }

        return results;
    }

    public async Task<Trip?> GetByIdAsync(Guid id)
    {
        var query = _container.GetItemQueryIterator<Trip>(
            new QueryDefinition("select * from c where c.id = @id")
            .WithParameter("@id", id.ToString()));

        var page = await query.ReadNextAsync();

        return page.FirstOrDefault();
    }

    public async Task UpdateAsync(Trip trip)
    {
        await _container.ReplaceItemAsync(
            trip,
            trip.Id.ToString(),
            new PartitionKey(trip.Country));
    }

    public async Task DeleteAsync(Guid id)
    {
        var trip = await GetByIdAsync(id);

        if (trip == null) return;

        await _container.DeleteItemAsync<Trip>(
            id.ToString(),
            new PartitionKey(trip.Country));
    }
}