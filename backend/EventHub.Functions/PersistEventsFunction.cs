using System.Text.Json;
using EventHub.Core.Abstractions;
using EventHub.Core.DTOs;
using EventHub.Core.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EventHub.Functions;

public sealed class PersistEventsFunction(IEventRepository repository, ILogger<PersistEventsFunction> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Function(nameof(PersistEventsFunction))]
    public async Task Run(
        [ServiceBusTrigger("events", Connection = "ServiceBusConnection")] string messageBody,
        CancellationToken cancellationToken)
    {
        EventDto dto;
        try
        {
            dto = JsonSerializer.Deserialize<EventDto>(messageBody, JsonOptions)
                ?? throw new InvalidOperationException("Payload deserialized to null.");
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Malformed JSON on Service Bus message; will retry or dead-letter.");
            throw;
        }

        if (dto.Id == Guid.Empty)
        {
            logger.LogError("Event payload has empty Id.");
            throw new InvalidOperationException("Event Id is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.UserId))
        {
            logger.LogError("Event payload has empty UserId.");
            throw new InvalidOperationException("Event UserId is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            logger.LogError("Event payload has empty Description.");
            throw new InvalidOperationException("Event Description is required.");
        }

        var entity = new Event
        {
            Id = dto.Id,
            UserId = dto.UserId,
            Type = dto.Type,
            Description = dto.Description,
            CreatedAt = dto.CreatedAt,
        };

        var inserted = await repository.AddAsync(entity, cancellationToken);
        if (inserted)
            logger.LogInformation("Persisted event {EventId} from queue", dto.Id);
        else
            logger.LogInformation("Event {EventId} already in database; idempotent completion", dto.Id);
    }
}
