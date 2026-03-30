using EventHub.Core.Abstractions;
using EventHub.Core.DTOs;
using EventHub.Core.Entities;
using EventHub.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventHub.Infrastructure.Repositories;

public sealed class EventRepository(
    IDbContextFactory<AppDbContext> dbFactory,
    ILogger<EventRepository> logger) : IEventRepository
{
    public async Task<EventDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var entity = await db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<PagedResult<EventDto>> GetPagedAsync(EventFilterDto filter, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var query = db.Events.AsNoTracking().AsQueryable();
        query = ApplyFilter(query, filter);
        query = ApplySort(query, filter);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(e => new EventDto
            {
                Id = e.Id,
                UserId = e.UserId,
                Type = e.Type,
                Description = e.Description,
                CreatedAt = e.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<EventDto>
        {
            Items = items,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize,
        };
    }

    public async Task<bool> AddAsync(Event entity, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        db.Events.Add(entity);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (IsSqlUniqueConstraintViolation(ex))
        {
            logger.LogWarning("Event {EventId} already exists in the database — idempotent skip", entity.Id);
            return false;
        }
    }

    private static bool IsSqlUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException sql && sql.Number is 2601 or 2627;

    private static IQueryable<Event> ApplyFilter(IQueryable<Event> query, EventFilterDto filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.UserIdContains))
            query = query.Where(e => e.UserId.Contains(filter.UserIdContains));

        if (filter.Type is { } type)
            query = query.Where(e => e.Type == type);

        if (filter.CreatedFrom is { } from)
            query = query.Where(e => e.CreatedAt >= from);

        if (filter.CreatedTo is { } to)
            query = query.Where(e => e.CreatedAt <= to);

        return query;
    }

    private static IQueryable<Event> ApplySort(IQueryable<Event> query, EventFilterDto filter)
    {
        return (filter.SortBy, filter.SortDescending) switch
        {
            (EventSortField.CreatedAt, true)  => query.OrderByDescending(e => e.CreatedAt),
            (EventSortField.CreatedAt, false) => query.OrderBy(e => e.CreatedAt),
            (EventSortField.UserId,    true)  => query.OrderByDescending(e => e.UserId),
            (EventSortField.UserId,    false) => query.OrderBy(e => e.UserId),
            (EventSortField.Type,      true)  => query.OrderByDescending(e => e.Type),
            (EventSortField.Type,      false) => query.OrderBy(e => e.Type),
            _                                 => query.OrderByDescending(e => e.CreatedAt),
        };
    }

    private static EventDto ToDto(Event e) => new()
    {
        Id = e.Id,
        UserId = e.UserId,
        Type = e.Type,
        Description = e.Description,
        CreatedAt = e.CreatedAt,
    };
}
