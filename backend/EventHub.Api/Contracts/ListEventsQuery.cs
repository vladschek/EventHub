using System.ComponentModel.DataAnnotations;
using EventHub.Core.DTOs;
using EventHub.Core.Entities;

namespace EventHub.Api.Contracts;

public sealed class ListEventsQuery : IValidatableObject
{
    [MaxLength(256)]
    public string? UserId { get; set; }

    public EventType? Type { get; set; }

    public DateTimeOffset? FromDate { get; set; }

    public DateTimeOffset? ToDate { get; set; }

    public EventSortField SortBy { get; set; } = EventSortField.CreatedAt;

    public bool SortDescending { get; set; } = true;

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FromDate is { } from && ToDate is { } to && from > to)
            yield return new ValidationResult(
                "fromDate must be earlier than or equal to toDate.",
                [nameof(FromDate), nameof(ToDate)]);
    }

    public EventFilterDto ToFilter() => new()
    {
        UserIdContains = UserId,
        Type = Type,
        CreatedFrom = FromDate,
        CreatedTo = ToDate,
        SortBy = SortBy,
        SortDescending = SortDescending,
        Page = Page,
        PageSize = PageSize,
    };
}
