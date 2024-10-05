using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.Weekplans.DTOs;

public record CreateDayplanDTO
{
    public int CitizenId { get; set; }
    
    [Required]
    public required string Date { get; set; }
}