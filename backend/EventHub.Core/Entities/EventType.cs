namespace EventHub.Core.Entities;

/// <summary>
/// Stored as string in SQL (not int). JSON uses these member names (e.g. PageView).
/// </summary>
public enum EventType
{
    PageView,
    Click,
    Purchase,
}
