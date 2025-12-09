using Microsoft.Azure.Cosmos;
using Azure.Messaging.ServiceBus;
using TravelCheck.Application.Interfaces;
using TravelCheck.Application.Services;
using TravelCheck.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// add api controllers
builder.Services.AddControllers();

// add swagger support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --------------------------
// COSMOS DB CLIENT
// --------------------------
builder.Services.AddSingleton(_ =>
    new CosmosClient(
        builder.Configuration["CosmosDb:AccountEndpoint"],
        builder.Configuration["CosmosDb:AccountKey"])
);

// --------------------------
// REPOSITORIES
// --------------------------
builder.Services.AddSingleton<ITripRepository, CosmosTripRepository>();
builder.Services.AddSingleton<IOutboxRepository, CosmosOutboxRepository>();

// --------------------------
// APPLICATION SERVICES
// --------------------------
builder.Services.AddScoped<TripService>();

// --------------------------
// SERVICE BUS CLIENT
// --------------------------
builder.Services.AddSingleton(_ =>
    new ServiceBusClient(builder.Configuration["ServiceBus:ConnectionString"]));

var app = builder.Build();

// enable swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// redirect http to https
app.UseHttpsRedirection();

// add authorization middleware
app.UseAuthorization();

// map api routes
app.MapControllers();

// run application
app.Run();
