using GirafAPI.Data;
using GirafAPI.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

// This seeder creates a user for tests that require valid user references (like creating a new organization).
public class BasicUserSeeder : DbSeeder
{
    public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;
        var user = new GirafUser
        {
            FirstName = "BasicUserSeeder.cs",
            LastName = "ForTestingPurposes",
            Organizations = new List<GirafAPI.Entities.Organizations.Organization>()
        };
        dbContext.Users.Add(user);
        dbContext.SaveChanges();
    }
}
