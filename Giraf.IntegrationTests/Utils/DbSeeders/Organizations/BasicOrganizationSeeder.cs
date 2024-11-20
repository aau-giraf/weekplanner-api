using GirafAPI.Data;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Grades;
using GirafAPI.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

// This seeder sets up a single organization for testing GET, PUT, DELETE operations.
public class BasicOrganizationSeeder : DbSeeder
{
    public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;
        var organization = new Organization
        {
            Name = "Test Organization",
            Users = new List<GirafUser>(),
            Citizens = new List<Citizen>(),
            Grades = new List<Grade>()
        };
        dbContext.Organizations.Add(organization);
        dbContext.SaveChanges();
    }
}
