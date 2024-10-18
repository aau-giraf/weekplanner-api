using Microsoft.AspNetCore.Identity;

namespace GirafAPI.Entities.Users;

// Base class for users that stores data common between user types.
public class GirafUser : IdentityUser
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }

}