using GirafAPI.Data;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

public class BasicUserWithPasswordSeeder : DbSeeder
{
    private readonly UserManager<GirafUser> _userManager;
    public static readonly string SeededUserName = "basicuser";
    public static readonly string SeededUserPassword = "OldP@ssw0rd!";

    public BasicUserWithPasswordSeeder(UserManager<GirafUser> userManager)
    {
        _userManager = userManager;
    }

    public override void SeedData(DbContext context)
    {
        var user = new GirafUser
        {
            UserName = SeededUserName,
            Email = "basicuser@example.com",
            FirstName = "Basic",
            LastName = "User"
        };

        var result = _userManager.CreateAsync(user, SeededUserPassword).GetAwaiter().GetResult();

        if (!result.Succeeded)
        {
            throw new Exception($"Failed to create user {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}
