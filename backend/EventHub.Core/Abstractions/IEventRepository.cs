using EventHub.Core.DTOs;
using EventHub.Core.Entities;

namespace EventHub.Core.Abstractions;

public interface IEventRepository
{
    Task<EventDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<EventDto>> GetPagedAsync(EventFilterDto filter, CancellationToken cancellationToken = default);

    Task<bool> AddAsync(Event entity, CancellationToken cancellationToken = default);
}
