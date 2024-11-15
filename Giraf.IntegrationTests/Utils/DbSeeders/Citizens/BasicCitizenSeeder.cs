using Microsoft.EntityFrameworkCore;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Activities;
using GirafAPI.Data;

namespace Giraf.IntegrationTests.Utils.DbSeeders;


// Seeder to add a single citizen for basic testing
public class BasicCitizenSeeder : DbSeeder
{
    public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;

        // Seed organization using OrganizationSeeder
        var organizationSeeder = new OrganizationSeeder("Basic Test Organization");
        organizationSeeder.Seed(dbContext);

        // Get the seeded organization to associate with the citizen
        var organization = dbContext.Organizations.FirstOrDefault();

        dbContext.Citizens.Add(new Citizen
        {
            FirstName = "Anders",
            LastName = "And",
            Organization = organization!,
            Activities = new List<Activity>()
        });

        dbContext.SaveChanges();
    }
}