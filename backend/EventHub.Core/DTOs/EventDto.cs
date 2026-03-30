using EventHub.Core.Entities;

namespace EventHub.Core.DTOs;

public sealed class EventDto
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public EventType Type { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
