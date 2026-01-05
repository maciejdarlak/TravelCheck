using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using TravelCheck.Application.Interfaces;
using Microsoft.Extensions.Hosting;


namespace TravelCheck.Worker;

// transfers events from the database (Outbox) to the Service Bus
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var events = await _outbox.GetUnprocessedAsync();

                foreach (var evt in events)
                {
                    await _sender.SendMessageAsync(
                        new ServiceBusMessage(evt.Payload),
                        stoppingToken);

                    await _outbox.MarkProcessedAsync(evt.Id);
                }
            }
            catch (Exception)
            {
            }

            await Task.Delay(3000, stoppingToken);
        }
    }

}