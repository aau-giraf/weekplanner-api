using GirafAPI.Data;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

// This seeder creates a user and associates multiple organizations with them for the GET /organizations/user/{id} test.
public class UserWithOrganizationsSeeder : DbSeeder
{
    private readonly UserManager<GirafUser> _userManager;

    public UserWithOrganizationsSeeder(UserManager<GirafUser> userManager)
    {
        _userManager = userManager;
    }

    public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;

       
        var organizations = new List<Organization>
        {
            new() {
                Name = "Organization A",
                Users = new List<GirafUser>(),
                Citizens = new List<GirafAPI.Entities.Citizens.Citizen>(),
                Grades = new List<GirafAPI.Entities.Grades.Grade>()
            },
            new() {
                Name = "Organization B",
                Users = new List<GirafUser>(),
                Citizens = new List<GirafAPI.Entities.Citizens.Citizen>(),
                Grades = new List<GirafAPI.Entities.Grades.Grade>()
            }
        };

        dbContext.Organizations.AddRange(organizations);
        dbContext.SaveChanges();

        var user = new GirafUser
        {
            FirstName = "UserWithOrganizationSeeder",
            LastName = "TestingPurposes",
            UserName = "BasicUserUsername",
            Email = "BasicUser@email.com",
            
        };


        var result = _userManager.CreateAsync(user, "Password123!").GetAwaiter().GetResult();

        if (!result.Succeeded)
        {
            throw new Exception($"Failed to create user {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Associate the user with the organization
        foreach (var org in organizations)
        {
            org.Users.Add(user);
        }

        dbContext.SaveChanges();
    }
}
