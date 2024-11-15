using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Data;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class BasicUserWithPasswordSeeder : DbSeeder
{
    public const string SeededUserPassword = "P@ssw0rd!";

    public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;
        var user = new GirafUser
        {
            UserName = "basicuser",
            Email = "basicuser@example.com",
            FirstName = "Basic",
            LastName = "User",
            PasswordHash = new PasswordHasher<GirafUser>().HashPassword(null, SeededUserPassword)
        };
        dbContext.Users.Add(user);
        dbContext.SaveChanges();
    }
}
