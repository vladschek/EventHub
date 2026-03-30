using EventHub.Core.Abstractions;
using EventHub.Core.DTOs;
using Microsoft.Extensions.Logging;

namespace EventHub.Infrastructure.Messaging;

public sealed class NoOpEventPublisher(ILogger<NoOpEventPublisher> logger) : IEventPublisher
{
    public Task PublishAsync(EventDto eventDto, CancellationToken cancellationToken = default)
    {
        logger.LogWarning(
            "Service Bus publish skipped (ServiceBus:DisablePublishing). Event {EventId} was not enqueued.",
            eventDto.Id);
        return Task.CompletedTask;
    }
}
