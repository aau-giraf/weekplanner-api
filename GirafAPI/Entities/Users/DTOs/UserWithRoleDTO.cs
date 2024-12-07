using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.Users.DTOs;

public record UserWithRoleDTO(
    [Required] string Id,
    [Required] string Email,
    [Required] [StringLength(100)] string FirstName,
    [Required] [StringLength(100)] string LastName,
    string Role
);