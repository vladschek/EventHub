using System.Text.Json;
using Azure.Messaging.ServiceBus;
using EventHub.Core.Abstractions;
using EventHub.Core.DTOs;

namespace EventHub.Infrastructure.Messaging;

public sealed class ServiceBusEventPublisher(ServiceBusClient client) : IEventPublisher, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ServiceBusSender _sender = client.CreateSender(ServiceBusOptions.EventsQueueName);

    public async Task PublishAsync(EventDto eventDto, CancellationToken cancellationToken = default)
    {
        var body = JsonSerializer.Serialize(eventDto, JsonOptions);
        await _sender.SendMessageAsync(
            new ServiceBusMessage(body) { ContentType = "application/json" },
            cancellationToken);
    }

    public ValueTask DisposeAsync() => _sender.DisposeAsync();
}
