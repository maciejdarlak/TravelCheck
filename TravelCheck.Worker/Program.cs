using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TravelCheck.Application.Interfaces;
using TravelCheck.Application.Services;
using TravelCheck.Infrastructure.Integrations;
using TravelCheck.Infrastructure.Repositories;
using TravelCheck.Infrastructure.Services;
using TravelCheck.Worker;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        IConfiguration config = context.Configuration;

        // cosmos db
        services.AddSingleton(_ =>
            new CosmosClient(
                config["CosmosDb:AccountEndpoint"],
                config["CosmosDb:AccountKey"])
            );

        // repositories
        services.AddSingleton<ITripRepository, CosmosTripRepository>();
        services.AddSingleton<IOutboxRepository, CosmosOutboxRepository>();

        // application services
        services.AddScoped<TripService>();

        // risky countries (typed HttpClient)
        services.AddHttpClient<HttpRiskyCountryProvider>(client =>
        {
            // client.BaseAddress = new Uri(config["RiskyCountries:BaseUrl"]);
        });

        services.AddScoped<IRiskyCountryService>(sp =>
        {
            var inner = sp.GetRequiredService<HttpRiskyCountryProvider>();
            var logger = sp.GetRequiredService<ILogger<RiskyCountryResilienceDecorator>>();
            return new RiskyCountryResilienceDecorator(inner, logger);
        });

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
