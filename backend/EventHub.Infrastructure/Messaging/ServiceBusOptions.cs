namespace EventHub.Infrastructure.Messaging;

/// <summary>
/// Single queue name for new events — must match architecture and Function trigger.
/// </summary>
public static class ServiceBusOptions
{
    public const string EventsQueueName = "events";
}
