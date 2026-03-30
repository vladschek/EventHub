namespace EventHub.Core.Entities;

public sealed class Event
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public EventType Type { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
