using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.Users.DTOs;

public record CreateUserDTO
{
    [Required]
    [StringLength(50)]
    public required string UserName { get; set; }

    [Required]
    [StringLength(100)]
    public required string Password { get; set; }
}
