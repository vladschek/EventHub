using System.ComponentModel.DataAnnotations;
using EventHub.Core.Entities;

namespace EventHub.Core.DTOs;

public sealed class CreateEventDto
{
    [Required]
    [MaxLength(256)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public EventType Type { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
}
