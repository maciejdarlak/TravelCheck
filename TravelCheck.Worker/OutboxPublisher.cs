using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using TravelCheck.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        _sender = client.CreateSender(config["ServiceBus:QueueName"]);
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // downloading unprocessed events from Outbox
                var events = await _outbox.GetUnprocessedAsync();

                // publication of each event
                foreach (var evt in events)
                {
                    // LOGS
                    // new logs --> send to Application Insights
                    using var scope = _logger.BeginScope(new Dictionary<string, object?>
                    {
                        ["OutboxEventId"] = evt.Id,
                        ["EventType"] = evt.Type,
                        ["CorrelationId"] = evt.CorrelationId
                    });

                    // TRACE
                    ActivityContext parentContext = default;

                    if (!string.IsNullOrWhiteSpace(evt.TraceParent)) // evt.TraceParent (context from Outbox)
                    {
                        parentContext = ActivityContext.Parse(evt.TraceParent, evt.TraceState); // API + evt trace connection
                    }

                    // creating a new span (trace part) --> send to Application Insights
                    using var activity = ActivitySource.StartActivity(
                        "PublishOutboxEvent",
                        ActivityKind.Producer,
                        parentContext);

                    // SERVICE BUS MESSAGE
                    var msg = new ServiceBusMessage(evt.Payload) // evt body
                    {
                        Subject = evt.Type, // e.g. "TripCreated"
                        CorrelationId = evt.CorrelationId ?? evt.Id.ToString() // e.g. "REQ-20240101-0001" or evt.Id
                    };

                    if (!string.IsNullOrWhiteSpace(evt.TraceParent)) // evt.TraceParent = TraceId + SpanId + Flags
                        msg.ApplicationProperties["traceparent"] = evt.TraceParent;

                    if (!string.IsNullOrWhiteSpace(evt.TraceState)) // evt.TraceState = others
                        msg.ApplicationProperties["tracestate"] = evt.TraceState;

                    await _sender.SendMessageAsync(msg, stoppingToken); // send to Service Bus

                    await _outbox.MarkProcessedAsync(evt.Id, evt.Type);

                    _logger.LogInformation("Outbox event published and marked as processed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxPublisher failed while publishing events.");
            }

            await Task.Delay(3000, stoppingToken);
        }
    }
}
