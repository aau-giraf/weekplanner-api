using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.Users.DTOs;

public class CreateUserDTO
{
    [Required]
    [StringLength(50)]
    public string UserName { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Password { get; set; } = null!;
}
