using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.Citizens.DTOs;

// Data necessary to create a Citizen
public record CreateCitizenDTO(
    [Required][StringLength(50)] string FirstName,
    [Required][StringLength(20)] string LastName
);