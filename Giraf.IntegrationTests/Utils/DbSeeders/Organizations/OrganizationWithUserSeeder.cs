using GirafAPI.Data;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Grades;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

// This seeder creates an organization and associates a user with it for the RemoveUser tests.
public class OrganizationWithUserSeeder : DbSeeder
{
        public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;
        var user = new GirafUser
        {
            FirstName = "OrganizationWithUserSeeder",
            LastName = "ForTestingPurposes",
            Organizations = new List<Organization>()
        };
        var organization = new Organization
        {
            Name = "Organization With User",
            Users = new List<GirafUser> { user },
            Citizens = new List<Citizen>(),
            Grades = new List<Grade>()
        };
        dbContext.Organizations.Add(organization);
        dbContext.SaveChanges();
    }
}
