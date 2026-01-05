using Azure.Messaging.ServiceBus;
using System.Text.Json;
using TravelCheck.Application.Events;
using TravelCheck.Application.Services;
using TravelCheck.Domain.Enums;

namespace TravelCheck.Worker;

public class TripCreatedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory; // creates a new DI session (using ...) 
    private readonly ServiceBusProcessor _processor; // receives messages from the queue

    public TripCreatedConsumer(
        IServiceScopeFactory scopeFactory, 
        ServiceBusClient client, // connection to Azure Service Bus
        IConfiguration config)
    {
        _scopeFactory = scopeFactory;

        _processor = client.CreateProcessor(config["ServiceBus:QueueName"]); 
        _processor.ProcessMessageAsync += OnMessage; 
        _processor.ProcessErrorAsync += OnError;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _processor.StartProcessingAsync(stoppingToken);
    }

    // read the event (trip ID), get this trip object (using scope) and update the trip status
    private async Task OnMessage(ProcessMessageEventArgs args)
    {
        var evt = JsonSerializer.Deserialize<TripCreatedEvent>(args.Message.Body); // json --> c#
        var tripId = evt!.TripId; // trip ID from this event

        using var scope = _scopeFactory.CreateScope(); // new DI session
        var tripService = scope.ServiceProvider.GetRequiredService<TripService>(); // scoped TripService object

        await tripService.ChangeStatusAsync(tripId, TripStatus.Processing); // status processing

        try
        {
            await Task.Delay(3000);
            await tripService.ChangeStatusAsync(tripId, TripStatus.Completed); // staus completed
        }
        catch
        {
            await tripService.ChangeStatusAsync(tripId, TripStatus.Rejected); // staus rejected
        }

        await args.CompleteMessageAsync(args.Message); // received
    }

    private Task OnError(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception);
        return Task.CompletedTask;
    }
}
