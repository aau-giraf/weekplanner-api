using System.Security.Claims;
using GirafAPI.Data;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Grades;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

// This seeder creates a user and associates multiple organizations with them for the GET /organizations/user/{id} test.
public class UserWithOrganizationsSeeder(UserManager<GirafUser> userManager) : DbSeeder
{
    private readonly UserManager<GirafUser> _userManager = userManager;

    public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;

        var organizations = new List<Organization>
        {
            new() {
                Name = "Organization A",
                Users = new List<GirafUser>(),
                Citizens = new List<Citizen>(),
                Grades = new List<Grade>()
            },
            new() {
                Name = "Organization B",
                Users = new List<GirafUser>(),
                Citizens = new List<Citizen>(),
                Grades = new List<Grade>()
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

        // Assign claims and associate user with organizations
        foreach (var org in organizations)
        {
            org.Users.Add(user);

            var memberClaim = new Claim("OrgMember", org.Id.ToString());
            _userManager.AddClaimAsync(user, memberClaim).GetAwaiter().GetResult();
        }

        dbContext.SaveChanges();
    }
}