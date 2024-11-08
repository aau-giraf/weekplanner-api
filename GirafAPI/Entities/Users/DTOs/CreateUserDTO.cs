using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.Users.DTOs;

public record CreateUserDTO
{
    [Required]
    [StringLength(50)]
    public required string Email { get; set; }

    [Required]
    [StringLength(100)]
    public required string Password { get; set; }
    
    [StringLength(20)]
    public required string FirstName { get; set; }
    
    [StringLength(50)]
    public required string LastName { get; set; }
    
}
