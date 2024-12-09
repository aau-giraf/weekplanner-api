using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.Activities.DTOs;

public record UpdateActivityDTO
(
    [Required] int CitizenId,
    [Required][StringLength(10)] string Date,
    [Required][StringLength(10)] string StartTime,
    [Required][StringLength(10)] string EndTime,
    [Required] bool IsCompleted,
    int? PictogramId
);
