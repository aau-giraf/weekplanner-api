using System.ComponentModel.DataAnnotations;
using GirafAPI.Entities.Pictograms;

namespace GirafAPI.Entities.Activities.DTOs;

public record ActivityDTO(
    [Required] int ActivityId,
    [Required][StringLength(10)] string Date,
    [Required][StringLength(50)] string Name,
    [StringLength(500)] string Description,
    [Required][StringLength(10)] string StartTime,
    [Required][StringLength(10)] string EndTime,
    bool IsCompleted,
    Pictogram pictogram
    );