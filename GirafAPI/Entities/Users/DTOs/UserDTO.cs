using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.DTOs;

public record UserDTO(
    [Required] string Id,
    [Required] string UserName,
    [Required][StringLength(100)] string FirstName,
    [Required][StringLength(100)] string LastName
);