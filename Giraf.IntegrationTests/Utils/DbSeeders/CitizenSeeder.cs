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
        organizationSeeder.SeedData(dbContext);

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

// Seeder to add multiple citizens for testing listing and retrieval
public class MultipleCitizensSeeder : DbSeeder
{
    public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;

        // Seed organization using OrganizationSeeder
        var organizationSeeder = new OrganizationSeeder("Multiple Citizens Test Organization");
        organizationSeeder.SeedData(dbContext);

        // Get the seeded organization to associate with the citizens
        var organization = dbContext.Organizations.FirstOrDefault();

        dbContext.Citizens.AddRange(new List<Citizen>
        {
            new Citizen { FirstName = "Anne", LastName = "Andersen", Organization = organization!, Activities = new List<Activity>() },
            new Citizen { FirstName = "Bent", LastName = "Bøje", Organization = organization!, Activities = new List<Activity>() },
            new Citizen { FirstName = "Charles", LastName = "Clarkson", Organization = organization!, Activities = new List<Activity>() }
        });

        dbContext.SaveChanges();
    }
}

// Seeder to add a citizen with an associated organization
public class CitizenWithOrganizationSeeder : DbSeeder
{
    public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;

        // Seed organization using OrganizationSeeder
        var organizationSeeder = new OrganizationSeeder("Specific Test Organization");
        organizationSeeder.SeedData(dbContext);

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