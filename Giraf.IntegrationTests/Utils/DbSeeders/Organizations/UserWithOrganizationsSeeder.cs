using GirafAPI.Data;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

// This seeder creates a user and associates multiple organizations with them for the GET /organizations/user/{id} test.
public class UserWithOrganizationsSeeder : DbSeeder
{
    public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;
        var user = new GirafUser
        {
            FirstName = "UserWithOrganizationSeeder",
            LastName = "TestingPurposes",
            UserName = "BasicUserUsername",
            Email = "BasicUser@email.com",
            Organizations = new List<Organization>
            {
                new Organization
                {
                    Name = "Organization A",
                    Users = new List<GirafUser>(),
                    Citizens = new List<GirafAPI.Entities.Citizens.Citizen>(),
                    Grades = new List<GirafAPI.Entities.Grades.Grade>()
                },
                new Organization
                {
                    Name = "Organization B",
                    Users = new List<GirafUser>(),
                    Citizens = new List<GirafAPI.Entities.Citizens.Citizen>(),
                    Grades = new List<GirafAPI.Entities.Grades.Grade>()
                }
            }
        };
        dbContext.Users.Add(user);
        dbContext.SaveChanges();
    }
}
