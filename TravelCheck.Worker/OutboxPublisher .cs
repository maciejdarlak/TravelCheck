using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using TravelCheck.Application.Interfaces;
using Microsoft.Extensions.Hosting;


namespace TravelCheck.Worker;

public class OutboxPublisher : BackgroundService
{
    private readonly IOutboxRepository _outbox;
    private readonly ServiceBusSender _sender;

    public OutboxPublisher(
        IOutboxRepository outbox, // Outbox repository
        ServiceBusClient client, // ServiceBus connection
        IConfiguration config) // appsettings.json
    {
        _outbox = outbox;
        _sender = client.CreateSender(config["ServiceBus:QueueName"]); // create Service Bus sender
    }

    protected override async Task ExecuteAsync (CancellationToken stoppingToken)
    {
        if (!stoppingToken.IsCancellationRequested)
        {
            var events = await _outbox.GetUnprocessedAsync(); // get unprocessed events

            foreach (var evt in events)
            {
                await _sender.SendMessageAsync(new ServiceBusMessage(evt.Payload)); // send event message
                await _outbox.MarkProcessedAsync(evt.Id); // mark it as processed
            }
        }

        await Task.Delay(3000, stoppingToken);
    }
}