using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Data;
using GirafAPI.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;

namespace Giraf.IntegrationTests.Utils;

// This factory creates a Giraf web API configured for testing.
internal class GirafWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Func<IServiceProvider, DbSeeder> _seederFactory;

    public GirafWebApplicationFactory(Func<IServiceProvider, DbSeeder> seederFactory)
    {
        _seederFactory = seederFactory;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing"); // Set the environment to "Testing"
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<GirafDbContext>));

            // Use a unique SQLite database file for each test to avoid concurrency issues
            var dbFileName = $"GirafTestDb_{Guid.NewGuid()}.db";

            // Configure the DbContext for testing
            services.AddDbContext<GirafDbContext>(options =>
            {
                options.UseSqlite($"Data Source={dbFileName}");
            });

            // Configure JwtSettings for testing
            services.Configure<JwtSettings>( options =>
            {
                options.Issuer = "TestIssuer";
                options.Audience = "TestAudience";
                options.SecretKey = "ThisIsASecretKeyForTestingPurposes!";
            });

            // Add the test authentication scheme
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.TestAuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.TestAuthenticationScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.TestAuthenticationScheme, options => { });

            // Add authorization policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("OrgMember", policy =>
                {
                    policy.RequireClaim("OrgMember");
                });

                options.AddPolicy("OrgAdmin", policy =>
                {
                    policy.RequireClaim("OrgAdmin");
                });
            });

            // Build the service provider and create a scope
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();

            // Clear the database before seeding
            dbContext.Database.EnsureDeleted();

            // Use migrations to apply schema, especially for identity tables
            dbContext.Database.Migrate();

            // Seed the database with scenario-specific data
            var seeder = _seederFactory(scope.ServiceProvider);
            seeder.SeedData(dbContext);
        });
    }
}