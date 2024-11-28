using System.Security.Claims;
using GirafAPI.Data;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Grades;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

// This seeder creates an organization and associates a user with it for the RemoveUser tests.
public class OrganizationWithUserSeeder : DbSeeder
{
    private readonly UserManager<GirafUser> _userManager;

    public OrganizationWithUserSeeder(UserManager<GirafUser> userManager)
    {
        _userManager = userManager;
    }

    public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;
        
        var organization = new Organization
        {
            Name = "Organization With User",
            Users = new List<GirafUser>(),
            Citizens = new List<Citizen>(),
            Grades = new List<Grade>()
        };

        dbContext.Organizations.Add(organization);
        dbContext.SaveChanges();

        // Reload the organization to ensure it's tracked
        var organizationFromDb = dbContext.Organizations.Include(o => o.Users).First(o => o.Id == organization.Id);

        var user = new GirafUser
        {
            FirstName = "OrganizationWithUserSeeder",
            LastName = "ForTestingPurposes",
            UserName = "BasicUserUsername",
            Email = "BasicUser@email.com",
        };

        var result = _userManager.CreateAsync(user, "Password123!").GetAwaiter().GetResult();

        if (!result.Succeeded)
        {
            throw new Exception($"Failed to create user {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Assign claims to the user
        var memberClaim = new Claim("OrgMember", organization.Id.ToString());
        var adminClaim = new Claim("OrgAdmin", organization.Id.ToString());

        _userManager.AddClaimAsync(user, memberClaim).GetAwaiter().GetResult();
        _userManager.AddClaimAsync(user, adminClaim).GetAwaiter().GetResult();

        // Reload the user to ensure it's tracked
        var userFromDb = dbContext.Users.Include(u => u.Organizations).First(u => u.Id == user.Id);

        // Associate the user with the organization
        organizationFromDb.Users.Add(userFromDb);
        dbContext.SaveChanges();
    }
}
