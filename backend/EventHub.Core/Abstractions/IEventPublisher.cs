using EventHub.Core.DTOs;

namespace EventHub.Core.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync(EventDto eventDto, CancellationToken cancellationToken = default);
}
