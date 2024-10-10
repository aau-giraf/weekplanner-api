using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.Weekplans.DTOs;

public record ActivityDTO(
    [Required] int ActivityId,
    [Required] int CitizenId,
    [Required][StringLength(10)] string Date,
    [Required][StringLength(50)] string Name,
    [StringLength(500)] string Description,
    [Required][StringLength(10)] string StartTime,
    [Required][StringLength(10)] string EndTime,
    [Required] bool IsCompleted
    );