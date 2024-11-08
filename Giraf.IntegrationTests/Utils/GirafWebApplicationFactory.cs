using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Giraf.IntegrationTests.Utils;

// This factory creates a Giraf web api configured for testing.
internal class GirafWebApplicationFactory(DbSeeder seeder) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<GirafDbContext>));

            // Add a Sqlite database for testing.
            services.AddDbContext<GirafDbContext>(options =>
            {
                options.UseSqlite("Data Source=GirafTestDb.db");
            });

            // Build scoped database context
            var serviceProvider = services.BuildServiceProvider();
            var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            
            // Seed test database
            seeder.SeedData(dbContext);
        });
    }
}