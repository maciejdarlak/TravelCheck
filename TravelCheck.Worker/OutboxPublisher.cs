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
    // ===== Telemetria / źródło aktywności (spans) =====
    private static readonly ActivitySource ActivitySource = new("TravelCheck.OutboxPublisher");

    // ===== Zależności (repo, sender, logger) =====
    private readonly IOutboxRepository _outbox;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<OutboxPublisher> _logger;

    // ===== Inicjalizacja / konstruktor =====
    public OutboxPublisher(
        IOutboxRepository outbox,
        ServiceBusClient client,
        IConfiguration config,
        ILogger<OutboxPublisher> logger)
    {
        _outbox = outbox;

        // Konfiguracja nadawcy Service Bus (queue)
        _sender = client.CreateSender(config["ServiceBus:QueueName"]);

        _logger = logger;
    }

    // ===== Główna pętla pracy BackgroundService =====
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
                    // adding additional items for each log (log = what happend)
                    using var scope = _logger.BeginScope(new Dictionary<string, object?>
                    {
                        ["OutboxEventId"] = evt.Id,
                        ["EventType"] = evt.Type,
                        ["CorrelationId"] = evt.CorrelationId
                    });

                    // trace context
                    ActivityContext parentContext = default;

                    // jeśli traceparent istnieje, kontynuujemy trace
                    if (!string.IsNullOrWhiteSpace(evt.TraceParent))
                    {
                        parentContext = ActivityContext.Parse(evt.TraceParent, evt.TraceState);
                    }

                    // this is a span - a single trace element (span = details: time, connection woth another spans ...)
                    using var activity = ActivitySource.StartActivity(
                        "PublishOutboxEvent",
                        ActivityKind.Producer,
                        parentContext);

                    // service bus message construction
                    var msg = new ServiceBusMessage(evt.Payload)
                    {
                        Subject = evt.Type,
                        CorrelationId = evt.CorrelationId ?? evt.Id.ToString()
                    };

                    // trace --> service bus message
                    if (!string.IsNullOrWhiteSpace(evt.TraceParent))
                        msg.ApplicationProperties["traceparent"] = evt.TraceParent;

                    if (!string.IsNullOrWhiteSpace(evt.TraceState))
                        msg.ApplicationProperties["tracestate"] = evt.TraceState;

                    // trace --> service bus 
                    await _sender.SendMessageAsync(msg, stoppingToken);                 
                    await _outbox.MarkProcessedAsync(evt.Id, evt.Type);

                    // succcess log 
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
