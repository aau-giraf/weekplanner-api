using GirafAPI.Data;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task SeedDataAsync(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Create roles
                var roles = new[] { "Administrator", "Trustee" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Create an admin user
                var adminUser = await userManager.FindByNameAsync("admin");
                if (adminUser == null)
                {
                    adminUser = new GirafUser { UserName = "admin"};
                    await userManager.CreateAsync(adminUser, "AdminPassword123!");
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                }

                // Create a trustee user
                var trusteeUser = await userManager.FindByNameAsync("trustee");
                if (trusteeUser == null)
                {
                    trusteeUser = new GirafUser { UserName = "trustee"};
                    await userManager.CreateAsync(trusteeUser, "TrusteePassword123!");
                    await userManager.AddToRoleAsync(trusteeUser, "Trustee");
                }
            }
        }

        public static async Task ApplyMigrationsAsync(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                await dbContext.Database.MigrateAsync();
            }
        }
    }
}
