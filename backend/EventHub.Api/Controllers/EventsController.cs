using EventHub.Api.Contracts;
using EventHub.Api.Hubs;
using EventHub.Core.Abstractions;
using EventHub.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace EventHub.Api.Controllers;

// No application service layer is introduced intentionally. Controller actions contain
// no domain business logic only infrastructure orchestration assign id,
// enqueue, broadcast. If validation rules enrichment or multi-step workflows are added
// in the future extract an IEventService at that point.
[ApiController]
[Route("api/events")]
public sealed class EventsController(
    IEventPublisher publisher,
    IEventRepository repository,
    IHubContext<EventsHub, IEventsHubClient> eventsHub,
    ILogger<EventsController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<EventDto>>> List([FromQuery] ListEventsQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Listing events: page={Page} pageSize={PageSize} userId={UserId} type={Type}",
            query.Page, query.PageSize, query.UserId, query.Type);

        var result = await repository.GetPagedAsync(query.ToFilter(), cancellationToken);

        logger.LogInformation("Returning {Count}/{Total} events", result.Items.Count, result.TotalCount);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await repository.GetByIdAsync(id, cancellationToken);

        if (dto is null)
        {
            logger.LogWarning("Event {EventId} not found", id);
            return NotFound();
        }

        return Ok(dto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventDto>> Create([FromBody] CreateEventDto body, CancellationToken cancellationToken)
    {
        var dto = new EventDto
        {
            Id = Guid.NewGuid(),
            UserId = body.UserId,
            Type = body.Type,
            Description = body.Description,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        logger.LogInformation(
            "Accepting event {EventId} for user {UserId} of type {EventType}",
            dto.Id, dto.UserId, dto.Type);

        await publisher.PublishAsync(dto, cancellationToken);

        logger.LogInformation("Event {EventId} published to queue", dto.Id);

        await eventsHub.Clients.All.EventCreated(dto);

        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }
}
