using Microsoft.AspNetCore.SignalR;

namespace EventHub.Api.Hubs;

/// <summary>Real-time channel for new events. Clients subscribe; server invokes <c>EventCreated</c>.</summary>
public sealed class EventsHub : Hub<IEventsHubClient>
{
}
