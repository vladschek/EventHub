using EventHub.Core.DTOs;

namespace EventHub.Api.Hubs;

/// <summary>Typed client contract for <see cref="EventsHub"/>. Enables compile-time safety on hub invocations.</summary>
public interface IEventsHubClient
{
    Task EventCreated(EventDto dto);
}
