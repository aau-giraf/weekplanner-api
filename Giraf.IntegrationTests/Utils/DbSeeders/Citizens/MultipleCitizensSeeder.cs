using Microsoft.EntityFrameworkCore;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Activities;
using GirafAPI.Data;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using GirafAPI.Entities.Grades;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

// Seeder to add multiple citizens for testing listing and retrieval
public class MultipleCitizensSeeder : DbSeeder
{
    public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;

        var organization = new Organization
        {
            Name = "Test Organization for Citizen",
            Users = new List<GirafUser>(),
            Citizens = new List<Citizen>(),
            Grades = new List<Grade>()
        };

        // Add an organization to the context
        dbContext.Organizations.Add(organization);
        dbContext.SaveChanges();

        dbContext.Citizens.AddRange(new List<Citizen>
        {
            new Citizen { FirstName = "Anne", LastName = "Andersen", Organization = organization!, Activities = new List<Activity>() },
            new Citizen { FirstName = "Bent", LastName = "Bøje", Organization = organization!, Activities = new List<Activity>() },
            new Citizen { FirstName = "Charles", LastName = "Clarkson", Organization = organization!, Activities = new List<Activity>() }
        });

        dbContext.SaveChanges();
    }
}