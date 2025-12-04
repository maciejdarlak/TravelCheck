using Microsoft.Azure.Cosmos;
using TravelCheck.Application.Interfaces;
using TravelCheck.Application.Services;
using TravelCheck.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// add api controllers
builder.Services.AddControllers();

// add swagger support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// register cosmos db client
builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var endpoint = config["CosmosDb:AccountEndpoint"];
    var key = config["CosmosDb:AccountKey"];
    return new CosmosClient(endpoint, key);
});

// register trip repository
builder.Services.AddSingleton<ITripRepository, CosmosTripRepository>();

// register application service
builder.Services.AddScoped<TripService>();

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
