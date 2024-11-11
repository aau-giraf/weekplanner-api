using System.ComponentModel.DataAnnotations;
using GirafAPI.Entities.Organizations;
using Microsoft.AspNetCore.Identity;

namespace GirafAPI.Entities.Users;

// Base class for users that stores data common between user types.
public class GirafUser : IdentityUser
{
    [StringLength(20)] [Required] public required string FirstName { get; set; }
    
    [StringLength(50)] [Required] public required string LastName { get; set; }
    
    public ICollection<Organization>? Organizations { get; set; }
}