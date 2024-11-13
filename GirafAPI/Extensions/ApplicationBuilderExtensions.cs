using GirafAPI.Data;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task SeedDataAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>();

            // Create a user
            var user = await userManager.FindByNameAsync("user");
            if (user == null)
            {
                user = new GirafUser { UserName = "user", 
                                       Email = "user@user.com", 
                                       FirstName = "User", 
                                       LastName = "Userson", 
                                       Organizations = new List<Organization>() };
                await userManager.CreateAsync(user, "Password123");
            }
        }

        public static async Task ApplyMigrationsAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
}