using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelCheck.Application.Interfaces;
using TravelCheck.Application.Services;
using TravelCheck.Infrastructure.Repositories;
using TravelCheck.Infrastructure.Services;
using TravelCheck.Worker;
using Microsoft.Extensions.DependencyInjection;


Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        IConfiguration config = context.Configuration;

        // cosmos db
        services.AddSingleton(_ =>
            new CosmosClient(
                config["CosmosDb:AccountEndpoint"],
                config["CosmosDb:AccountKey"]));

        // repositories
        services.AddSingleton<ITripRepository, CosmosTripRepository>();
        services.AddSingleton<IOutboxRepository, CosmosOutboxRepository>();

        // application services
        services.AddScoped<TripService>();

        // external source (future: ONZ/MSZ HTTP)
        services.AddHttpClient(); 

        // risky countries (offline impl now, replace later with real HTTP/DB)
        services.AddSingleton<IRiskyCountryService, FakeRiskyCountryService>(); 

        // azure service bus
        services.AddSingleton(_ =>
            new ServiceBusClient(config["ServiceBus:ConnectionString"]));

        // outbox worker
        services.AddHostedService<OutboxPublisher>();

        // event consumer
        services.AddHostedService<TripCreatedConsumer>();
    })
    .Build()
    .Run();
