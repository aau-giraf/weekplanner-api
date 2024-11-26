using GirafAPI.Data;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

// This seeder creates a user for tests that require valid user references (like creating a new organization).
public class BasicUserSeeder : DbSeeder
{
    private readonly UserManager<GirafUser> _userManager;

    public BasicUserSeeder(UserManager<GirafUser> userManager)
    {
        _userManager = userManager;
    }

    public override void SeedData(DbContext context)
    {
        var user = new GirafUser
        {
            UserName = "BasicUserUsername",
            Email = "BasicUser@email.com",
            FirstName = "BasicUserSeeder.cs",
            LastName = "ForTestingPurposes",
            Organizations = new List<Organization>()
        };


        var result = _userManager.CreateAsync(user, "Password123!").GetAwaiter().GetResult();

        if (!result.Succeeded)
        {
            throw new Exception("Failed to create user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
