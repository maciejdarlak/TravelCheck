using Microsoft.Azure.Cosmos;
using Azure.Messaging.ServiceBus;
using TravelCheck.Application.Interfaces;
using TravelCheck.Application.Services;
using TravelCheck.Infrastructure.Repositories;
using Microsoft.ApplicationInsights.Extensibility;

var builder = WebApplication.CreateBuilder(args);

// api controllers
builder.Services.AddControllers();

// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// application insights
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString =
        builder.Configuration["ApplicationInsights:ConnectionString"];
});

// cosmos db client
builder.Services.AddSingleton(_ =>
    new CosmosClient(
        builder.Configuration["CosmosDb:AccountEndpoint"],
        builder.Configuration["CosmosDb:AccountKey"])
);

// repositories
builder.Services.AddSingleton<ITripRepository, CosmosTripRepository>();
builder.Services.AddSingleton<IOutboxRepository, CosmosOutboxRepository>();

// application services
builder.Services.AddScoped<TripService>();

// service bus client
builder.Services.AddSingleton(_ =>
    new ServiceBusClient(
        builder.Configuration["ServiceBus:ConnectionString"])
);

var app = builder.Build();

// swagger (dev only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// https
app.UseHttpsRedirection();

// authorization
app.UseAuthorization();

// api routes
app.MapControllers();

// run app
app.Run();
