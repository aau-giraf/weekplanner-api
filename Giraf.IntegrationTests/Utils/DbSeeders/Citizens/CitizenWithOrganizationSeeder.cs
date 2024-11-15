using Microsoft.EntityFrameworkCore;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Activities;
using GirafAPI.Data;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

// Seeder to add a citizen with an associated organization
public class CitizenWithOrganizationSeeder : DbSeeder
{
    public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;

        // Seed organization using OrganizationSeeder
        var organizationSeeder = new OrganizationSeeder("Specific Test Organization");
        organizationSeeder.Seed(dbContext);

        // Get the seeded organization to associate with the citizen
        var organization = dbContext.Organizations.FirstOrDefault();

        dbContext.Citizens.Add(new Citizen
        {
            FirstName = "David",
            LastName = "Danielsen",
            Organization = organization!,
            Activities = new List<Activity>()
        });

        dbContext.SaveChanges();
    }
}