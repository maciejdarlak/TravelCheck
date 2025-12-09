using Azure.Messaging.ServiceBus;
using System.Text.Json;
using TravelCheck.Application.Events;
using TravelCheck.Application.Services;
using TravelCheck.Domain.Enums;

namespace TravelCheck.Worker;

public class TripCreatedConsumer : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly IServiceScopeFactory _scopeFactory;

    public TripCreatedConsumer(
        ServiceBusClient client, // connection to Azure Service Bus
        IServiceScopeFactory scopeFactory, // new DI scope for TripService
        IConfiguration config)
    {
        _scopeFactory = scopeFactory;

        _processor = client.CreateProcessor(config["ServiceBus:QueueName"]); // queue listener
        _processor.ProcessMessageAsync += OnMessage; // every message goes to OnMessage
        _processor.ProcessErrorAsync += OnError;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _processor.StartProcessingAsync(stoppingToken);
    }

    // read the event and update the trip status
    private async Task OnMessage(ProcessMessageEventArgs args)
    {
        var evt = JsonSerializer.Deserialize<TripCreatedEvent>(args.Message.Body); // json --> c#
        var tripId = evt!.TripId;

        using var scope = _scopeFactory.CreateScope(); // new DI session
        var tripService = scope.ServiceProvider.GetRequiredService<TripService>(); // getting TripService

        await tripService.ChangeStatusAsync(tripId, TripStatus.Processing); // status = Processing

        try
        {
            await Task.Delay(3000);
            await tripService.ChangeStatusAsync(tripId, TripStatus.Completed); // success
        }
        catch
        {
            await tripService.ChangeStatusAsync(tripId, TripStatus.Rejected); // error
        }

        await args.CompleteMessageAsync(args.Message); // received
    }

    private Task OnError(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception);
        return Task.CompletedTask;
    }
}
