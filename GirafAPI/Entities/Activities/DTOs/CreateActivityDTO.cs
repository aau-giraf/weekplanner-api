using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.Activities.DTOs;

public record CreateActivityDTO
(
    [Required]DateOnly Date,
    [Required][StringLength(50)] string Name,
    [StringLength(500)] string Description,
    [Required]TimeOnly StartTime,
    [Required]TimeOnly EndTime,
    int? PictogramId
);