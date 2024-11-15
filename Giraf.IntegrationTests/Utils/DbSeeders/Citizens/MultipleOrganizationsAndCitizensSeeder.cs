using Microsoft.EntityFrameworkCore;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Users;
using GirafAPI.Entities.Activities;
using GirafAPI.Entities.Organizations;
using GirafAPI.Data;
using GirafAPI.Entities.Grades;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

public class MultipleOrganizationsAndCitizensSeeder : DbSeeder
{
    public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;

        // Create two organizations
        var organization1 = new Organization { Name = "Organization One", Citizens = new List<Citizen>(), Users = new List<GirafUser>(), Grades = new List<Grade>()};
        var organization2 = new Organization { Name = "Organization Two", Citizens = new List<Citizen>(), Users = new List<GirafUser>(), Grades = new List<Grade>() };

        dbContext.Organizations.AddRange(organization1, organization2);

        // Add citizens to each organization
        var citizen1 = new Citizen
        {
            FirstName = "Anja",
            LastName = "Ansjos",
            Organization = organization1,
            Activities = new List<Activity>()
        };

        var citizen2 = new Citizen
        {
            FirstName = "Bølle",
            LastName = "Bob",
            Organization = organization2,
            Activities = new List<Activity>()
        };

        dbContext.Citizens.AddRange(citizen1, citizen2);

        dbContext.SaveChanges();
    }
}