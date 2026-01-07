using Azure.Messaging.ServiceBus;
using System.Text.Json;
using TravelCheck.Application.Events;
using TravelCheck.Application.Interfaces;
using TravelCheck.Application.Services;
using TravelCheck.Domain.Enums;

namespace TravelCheck.Worker;

public class TripCreatedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory; 
    private readonly ServiceBusProcessor _processor; 

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

    // Handles TripCreatedEvent messages: loads the trip aggregate, 
    // evaluates external risk rules, and finalizes the trip status accordingly.
    private async Task OnMessage(ProcessMessageEventArgs args)
    {
        TripCreatedEvent evt;

        try // json --> TripCreatedEvent
        {
            evt = args.Message.Body.ToObjectFromJson<TripCreatedEvent>() 
                  ?? throw new JsonException("Payload deserialized to null.");
        }
        catch (Exception ex) // if is not possible --> dead letter queue
        {
            await args.DeadLetterMessageAsync(args.Message, "InvalidPayload", ex.Message); 
            return;
        }

        using var scope = _scopeFactory.CreateScope(); // DI scope 

        var tripService = scope.ServiceProvider.GetRequiredService<TripService>(); // DI scope access to TripService
        var riskyCountryService = scope.ServiceProvider.GetRequiredService<IRiskyCountryService>(); // DI scope access to IRiskyCountryService

        try
        {
            // 1) load trip first - id from evt
            var trip = await tripService.GetByIdAsync(evt.TripId);

            if (trip is null)
            {
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            // 2) if already finalized break it 
            if (trip.Status == TripStatus.Completed || trip.Status == TripStatus.Rejected)
            {
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            // 3) move to processing only when New
            if (trip.Status == TripStatus.New)
            {
                await tripService.ChangeStatusAsync(evt.TripId, TripStatus.Processing);
            }

            // 4) external decision
            var isRisky = await riskyCountryService.IsCountryRiskyAsync(trip.Country, args.CancellationToken);

            // 5) finalize 
            await tripService.ChangeStatusAsync(
                evt.TripId,
                isRisky ? TripStatus.Rejected : TripStatus.Completed);

            // 6) remove from service bus queue
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            await args.AbandonMessageAsync(args.Message);
            Console.WriteLine(ex);
            throw;
        }
    }

    private Task OnError(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception);
        return Task.CompletedTask;
    }
}