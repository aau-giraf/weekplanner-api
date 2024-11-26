using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Data;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

public class MultipleUsersSeeder : DbSeeder
{
    private readonly UserManager<GirafUser> _userManager;

    public MultipleUsersSeeder(UserManager<GirafUser> userManager)
    {
        _userManager = userManager;
    }

    public override void SeedData(DbContext context)
    {
        var users = new List<GirafUser>
        {
            new GirafUser
            {
                UserName = "user1",
                Email = "user1@example.com",
                FirstName = "User",
                LastName = "One"
            },
            new GirafUser
            {
                UserName = "user2",
                Email = "user2@example.com",
                FirstName = "User",
                LastName = "Two"
            }
        };

        foreach (var user in users)
        {
            var result = _userManager.CreateAsync(user, "Password123!").GetAwaiter().GetResult();
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create user {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}