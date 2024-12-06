using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.Activities.DTOs;

public record CreateActivityDTO
(
    [Required]DateOnly Date,
    [Required]TimeOnly StartTime,
    [Required]TimeOnly EndTime,
    int? PictogramId
);