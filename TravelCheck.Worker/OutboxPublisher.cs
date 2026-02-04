using Azure.Messaging.ServiceBus;
using TravelCheck.Application.Interfaces;
using System.Diagnostics;

namespace TravelCheck.Worker;

// transfers events from the database (Outbox) to the Service Bus
public class OutboxPublisher : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("TravelCheck.OutboxPublisher"); // spans factory
    private readonly IOutboxRepository _outbox;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<OutboxPublisher> _logger;

    public OutboxPublisher(
        IOutboxRepository outbox, // CosmosOutboxRepository class implementation - access to events repository
        ServiceBusClient client,  // connection string from appsetings.json
        IConfiguration config,
        ILogger<OutboxPublisher> logger)
    {
        _outbox = outbox;
        _logger = logger;

        var queueName = config["ServiceBus:QueueName"]; // reads the service bus queue name from the configuration
        if (string.IsNullOrWhiteSpace(queueName))
            throw new InvalidOperationException("Missing configuration value: ServiceBus:QueueName"); // fail fast

        _sender = client.CreateSender(queueName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {              
                var events = await _outbox.GetUnprocessedAsync(); // downloading unprocessed events from Outbox

                foreach (var evt in events)
                {
                    stoppingToken.ThrowIfCancellationRequested(); // fast stop between messages

                    // ╔══════════════════════════════════════════════════════════════════════════╗
                    // ║      TELEMETRY                                                           ║
                    // ╚══════════════════════════════════════════════════════════════════════════╝

                    // trace
                    ActivityContext parentContext = default;

                    if (!string.IsNullOrWhiteSpace(evt.TraceParent))
                        parentContext = ActivityContext.Parse(evt.TraceParent, evt.TraceState);

                    using var activity = ActivitySource.StartActivity(
                        "PublishOutboxEvent",
                        ActivityKind.Producer,
                        parentContext);

                    activity?.SetTag("outbox.event_id", evt.Id.ToString());
                    activity?.SetTag("outbox.event_type", evt.Type);

                    // logs
                    using var scope = _logger.BeginScope(new Dictionary<string, object?>
                    {
                        ["OutboxEventId"] = evt.Id,
                        ["EventType"] = evt.Type,
                        ["CorrelationId"] = evt.CorrelationId
                    });

                    // ╔══════════════════════════════════════════════════════════════════════════╗
                    // ║      MESSAGE PROCESSING LOGIC (PUBLISH TO SERVICE BUS)                   ║
                    // ╚══════════════════════════════════════════════════════════════════════════╝

                    var msg = new ServiceBusMessage(evt.Payload) // evt body
                    {
                        MessageId = evt.Id.ToString(),
                        Subject = evt.Type,
                        CorrelationId = evt.CorrelationId ?? evt.Id.ToString()
                    };

                    // propagate trace to consumer
                    if (!string.IsNullOrWhiteSpace(evt.TraceParent))
                        msg.ApplicationProperties["traceparent"] = evt.TraceParent;

                    if (!string.IsNullOrWhiteSpace(evt.TraceState))
                        msg.ApplicationProperties["tracestate"] = evt.TraceState;

                    await _sender.SendMessageAsync(msg, stoppingToken); // send to Service Bus
                    await _outbox.MarkProcessedAsync(evt.Id, evt.Type); // mark processed

                    _logger.LogInformation("Outbox event published and marked as processed.");
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break; // normal shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxPublisher failed while publishing events.");
            }

            await Task.Delay(3000, stoppingToken); // polling
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // ensure sender is disposed
        await _sender.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
