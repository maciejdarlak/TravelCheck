using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Policy;
using System.Text.Json;
using TravelCheck.Application.Events;
using TravelCheck.Application.Interfaces;
using TravelCheck.Application.Services;
using TravelCheck.Domain.Enums;

namespace TravelCheck.Worker;

public sealed class TripCreatedConsumer : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("TravelCheck.TripCreatedConsumer");
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger<TripCreatedConsumer> _logger;

    public TripCreatedConsumer(
        IServiceScopeFactory scopeFactory,
        ServiceBusClient client,
        IConfiguration config, // appsettings.json access
        ILogger<TripCreatedConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var queueName = config["ServiceBus:QueueName"]; // reads the Service Bus queue name from the configuration
        if (string.IsNullOrWhiteSpace(queueName))
            throw new InvalidOperationException("Missing configuration value: ServiceBus:QueueName");

        _processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions // it listens to the queue in SB
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1,
            PrefetchCount = 0
        });

        _processor.ProcessMessageAsync += OnMessageAsync; // when the message comes use OnMessageAsync method
        _processor.ProcessErrorAsync += OnErrorAsync;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _processor.StartProcessingAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try { await _processor.StopProcessingAsync(cancellationToken); }
        finally
        {
            await _processor.DisposeAsync();
            await base.StopAsync(cancellationToken);
        }
    }

    private async Task OnMessageAsync(ProcessMessageEventArgs args) // sb message
    {

        // ╔══════════════════════════════════════════════════════════════════════════╗
        // ║      TELEMETRY                                                           ║
        // ╚══════════════════════════════════════════════════════════════════════════╝

        var (parentContext, hasParent) = TryGetParentActivityContext(args.Message.ApplicationProperties); // trace data  (context)

        // continue publisher trace or start a new one
        using var activity = ActivitySource.StartActivity(
            "ConsumeTripCreatedEvent",
            ActivityKind.Consumer,
            hasParent ? parentContext : default);

        // logscope makes titles of each log using "logScope" 
        using var logScope = _logger.BeginScope(new Dictionary<string, object?> 
        {
            ["MessageId"] = args.Message.MessageId,
            ["EventType"] = args.Message.Subject,
            ["CorrelationId"] = args.Message.CorrelationId,
            ["EntityPath"] = args.EntityPath
        });

        // ╔══════════════════════════════════════════════════════════════════════════╗
        // ║      MESSAGE PROCESSING LOGIC (AZURE SERVICE BUS CONSUMER)               ║
        // ╚══════════════════════════════════════════════════════════════════════════╝

        TripCreatedEvent evt;

        try
        {
            evt = args.Message.Body.ToObjectFromJson<TripCreatedEvent>() // json deserialisation from service bus --> TripCreatedEvent
                  ?? throw new JsonException("Payload deserialized to null.");

            activity?.SetTag("trip.id", evt.TripId.ToString()); // adding a tag with the trip ID value to the trace for each sb message
        }

        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Invalid payload. Sending message to dead letter queue.");
            activity?.SetStatus(ActivityStatusCode.Error, "InvalidPayload");
            await args.DeadLetterMessageAsync(args.Message, "InvalidPayload", ex.Message);
            return;
        }

        using var scope = _scopeFactory.CreateScope(); // access to DI container
        var tripService = scope.ServiceProvider.GetRequiredService<TripService>();
        var riskyCountryService = scope.ServiceProvider.GetRequiredService<IRiskyCountryService>();

        try
        {
            // load trip aggregate by id
            var trip = await tripService.GetByIdAsync(evt.TripId);

            if (trip is null
                || trip.Status is TripStatus.Completed or TripStatus.Rejected)
            {
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            // move to Processing only when New
            if (trip.Status == TripStatus.New)
                await tripService.ChangeStatusAsync(evt.TripId, TripStatus.Processing);

            // checks if the country is risky
            var isRisky = await riskyCountryService.IsCountryRiskyAsync(trip.Country, args.CancellationToken);

            // finalize trip status
            await tripService.ChangeStatusAsync(
                evt.TripId,
                isRisky ? TripStatus.Rejected : TripStatus.Completed);

            // remove from Service Bus queue
            await args.CompleteMessageAsync(args.Message);
            _logger.LogInformation("TripCreatedEvent processed successfully.");
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message); // telemetry shows error 
            _logger.LogError(ex, "TripCreatedConsumer failed while processing message. Abandoning.");
            await args.AbandonMessageAsync(args.Message); // mesage returns to sb queque
            return;
        }
    }

    private Task OnErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "ServiceBusProcessor error. ErrorSource={ErrorSource} EntityPath={EntityPath} FullyQualifiedNamespace={Fqn}",
            args.ErrorSource,
            args.EntityPath,
            args.FullyQualifiedNamespace);

        return Task.CompletedTask;
    }

    // read and parse traceparent/tracestate
    private static (ActivityContext context, bool hasContext) TryGetParentActivityContext(
        IReadOnlyDictionary<string, object> applicationProperties)
    {
        var traceParent = applicationProperties.TryGetValue("traceparent", out var tpObj)
            ? tpObj as string
            : null;

        if (string.IsNullOrWhiteSpace(traceParent))
            return (default, false);

        var traceState = applicationProperties.TryGetValue("tracestate", out var tsObj)
            ? tsObj as string
            : null;

        try 
        { 
            return (ActivityContext.Parse(traceParent, traceState), true); 
        }

        catch
        {
            return (default, false);
        }
    }
}
