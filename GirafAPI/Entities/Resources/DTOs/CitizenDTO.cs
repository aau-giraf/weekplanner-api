using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.Resources.DTOs;

public record CitizenDTO(
    int Id,
    [Required][StringLength(50)] string FirstName,
    [Required][StringLength(20)] string LastName,
    int WeekplanId
);