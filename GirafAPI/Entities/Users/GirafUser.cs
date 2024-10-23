using Microsoft.AspNetCore.Identity;
using GirafAPI.Entities.Organizations;

namespace GirafAPI.Entities.Users;

// Base class for users that stores data common between user types.
public class GirafUser : IdentityUser
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }

    public int? OrganizationId { get; set; }
    public Organization Organization { get; set; }
}