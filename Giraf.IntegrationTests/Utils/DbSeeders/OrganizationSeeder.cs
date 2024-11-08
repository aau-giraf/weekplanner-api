using Microsoft.EntityFrameworkCore;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Users;
using GirafAPI.Entities.Grades;
using GirafAPI.Data;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

public class OrganizationSeeder : DbSeeder
{
    private readonly string _organizationName;

    // Constructor to allow setting the organization name
    public OrganizationSeeder(string organizationName = "Test Organization")
    {
        _organizationName = organizationName;
    }

    public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;

        // Create an organization with required fields
        var organization = new Organization
        {
            Name = _organizationName,
            Citizens = new List<Citizen>(),
            Users = new List<GirafUser>(),
            Grades = new List<Grade>()
        };
        dbContext.Organizations.Add(organization);
        dbContext.SaveChanges();
    }
}
