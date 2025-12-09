using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using TravelCheck.Application.Interfaces;
using TravelCheck.Domain.Entities;

namespace TravelCheck.Infrastructure.Repositories;

public class CosmosOutboxRepository : IOutboxRepository
{
    private readonly Container _container;

    // container connection
    public CosmosOutboxRepository(CosmosClient client, IConfiguration cfg)
    {
        var db = cfg["CosmosDb:DatabaseName"]!;
        var containerName = cfg["CosmosDb:ContainerName"]!;
        _container = client.GetContainer(db, containerName);
    }

    // saving a new event to the Cosmos DB container
    public Task AddAsync(OutboxEvent evt)
        => _container.CreateItemAsync(evt, new PartitionKey(evt.Type));

    // Queries Cosmos DB and returns all records with processed = false
    public async Task<IEnumerable<OutboxEvent>> GetUnprocessedAsync()
    {
        var query = _container.GetItemQueryIterator<OutboxEvent>(
            new QueryDefinition("select * from c where c.procced == false"));

        var res = new List<OutboxEvent>();

        while (query.HasMoreResults)
            res.AddRange(await query.ReadNextAsync());

        return res;
    }

    // Update the processed field in the Cosmos DB document using the PATCH operation
    public Task MarkProcessedAsync(Guid id)
        => _container.PatchItemAsync<OutboxEvent>(id.ToString(),
            new PartitionKey("TripCreated"),
            new[] { PatchOperation.Replace("/processed", true) });
}