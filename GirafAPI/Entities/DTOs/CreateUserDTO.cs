using System.ComponentModel.DataAnnotations;
using GirafAPI.Utils;

namespace GirafAPI.Entities.Users.DTOs;

public class CreateUserDTO
{
    [Required]
    [StringLength(50)]
    public string UserName { get; set; }

    [Required]
    [StringLength(100)]
    public string Password { get; set; }
    
    public string Role { get; set; }
    
    [StringLength(100)]
    public string FirstName { get; set; }
    
    [StringLength(100)]
    public string LastName { get; set; }
}
