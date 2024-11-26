using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.Activities.DTOs;

public record UpdateActivityDTO
(
    [Required] int CitizenId,
    [Required]DateOnly Date,
    [Required][StringLength(50)] string Name,
    [StringLength(500)] string Description,
    [Required]TimeOnly StartTime,
    [Required]TimeOnly EndTime,
    [Required] bool IsCompleted,
    int PictogramId
);