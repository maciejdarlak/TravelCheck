using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
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

        // application insights (worker)
        services.AddApplicationInsightsTelemetryWorkerService(options =>
        {
            options.ConnectionString = config["ApplicationInsights:ConnectionString"];
        });

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
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseUrls("http://0.0.0.0:8080");
        webBuilder.ConfigureServices(services =>
        {
            services.AddHealthChecks();
        });
        webBuilder.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
            });
        });
    })
    .Build()
    .Run();
