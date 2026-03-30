using System.ComponentModel.DataAnnotations;
using EventHub.Core.Entities;

namespace EventHub.Core.DTOs;

public sealed class EventFilterDto
{
    public string? UserIdContains { get; set; }

    public EventType? Type { get; set; }

    public DateTimeOffset? CreatedFrom { get; set; }

    public DateTimeOffset? CreatedTo { get; set; }

    public EventSortField SortBy { get; set; } = EventSortField.CreatedAt;

    public bool SortDescending { get; set; } = true;

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}
