using GirafAPI.Data;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Giraf.IntegrationTests.Utils.DbSeeders
{
    public class LoginUserSeeder : DbSeeder
    {
        private readonly UserManager<GirafUser> _userManager;

        // Static fields to hold the seeded users credentials
        public static readonly string SeededUserName = "testuser";
        public static readonly string SeededUserPassword = "TestP@ssw0rd!";

        public LoginUserSeeder(UserManager<GirafUser> userManager)
        {
            _userManager = userManager;
        }

        public override void SeedData(DbContext context)
        {
            var user = new GirafUser
            {
                UserName = SeededUserName,
                Email = "testuser@example.com",
                FirstName = "Test",
                LastName = "User"
            };

            var result = _userManager.CreateAsync(user, SeededUserPassword).GetAwaiter().GetResult();

            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create user {user.UserName}: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
