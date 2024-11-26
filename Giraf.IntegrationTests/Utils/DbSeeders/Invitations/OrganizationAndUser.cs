

using GirafAPI.Data;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Grades;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

public class OrganizationAndUser : DbSeeder 
{
    private readonly UserManager<GirafUser> _userManager;

    public OrganizationAndUser(UserManager<GirafUser> userManager)
    {
        _userManager = userManager;
    }

    public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;

        var sender = new GirafUser
            {
                UserName = "SenderUser",
                Email = "SenderUser@example.com",
                FirstName = "Sender",
                LastName = "User",
                Organizations = new List<Organization>()
            };
        
        var result = _userManager.CreateAsync(sender, "Password123!").GetAwaiter().GetResult();

        if (!result.Succeeded)
        {
            throw new Exception("Failed to create user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        var organization = new Organization
            {
                Id = 123,
                Name = "Basic Test Invitation Organization",
                Users = new List<GirafUser> {sender},
                Citizens = new List<Citizen>(),
                Grades = new List<Grade>()
            };
        dbContext.Organizations.Add(organization);
        dbContext.SaveChanges();
    }
}
