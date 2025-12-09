using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelCheck.Application.Interfaces;
using TravelCheck.Application.Services;
using TravelCheck.Infrastructure.Repositories;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        IConfiguration config = context.Configuration;

        // Cosmos DB
        services.AddSingleton(_ =>
            new CosmosClient(
                config["CosmosDb:AccountEndpoint"],
                config["CosmosDb:AccountKey"]));

        // Repositories
        services.AddSingleton<ITripRepository, CosmosTripRepository>();
        services.AddSingleton<IOutboxRepository, CosmosOutboxRepository>();

        // Application services
        services.AddScoped<TripService>();

        // Azure Service Bus
        services.AddSingleton(_ =>
            new ServiceBusClient(config["ServiceBus:ConnectionString"]));

        // Outbox worker
        services.AddHostedService<OutboxPublisher>();
    })
    .Build()
    .Run();
