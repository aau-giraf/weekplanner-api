using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Data;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using Microsoft.EntityFrameworkCore;

public class MultipleUsersSeeder : DbSeeder
{
    public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;
        dbContext.Users.AddRange(new List<GirafUser>
        {
            new GirafUser
            {
                UserName = "user1",
                Email = "user1@example.com",
                FirstName = "User",
                LastName = "One",
                Organizations = new List<Organization>()
            },
            new GirafUser
            {
                UserName = "user2",
                Email = "user2@example.com",
                FirstName = "User",
                LastName = "Two",
                Organizations = new List<Organization>()
            }
        });
        dbContext.SaveChanges();
    }
}
