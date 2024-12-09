using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Authorization;
using GirafAPI.Data;
using GirafAPI.Configuration;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Giraf.IntegrationTests.Utils;

// This factory creates a Giraf web API configured for testing.
internal class GirafWebApplicationFactory : WebApplicationFactory<Program>
{

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
            services.AddScoped<IAuthorizationHandler, OrgMemberAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, OrgAdminAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, OrgOwnerAuthorizationHandler>();
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy("OrganizationMember", policy =>
                    policy.Requirements.Add(new OrgMemberRequirement()));
                options.AddPolicy("OrganizationAdmin", policy =>
                    policy.Requirements.Add(new OrgAdminRequirement()));
                options.AddPolicy("OrganizationOwner", policy =>
                    policy.Requirements.Add(new OrgOwnerRequirement()));
            });

            // Build the service provider and create a scope
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();

            // Clear the database before seeding
            dbContext.Database.EnsureDeleted();

            // Use migrations to apply schema, especially for identity tables
            dbContext.Database.Migrate();
        });
    }

    public void SeedDb(IServiceScope scope, DbSeeder seeder)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>();
        seeder.SeedData(dbContext, userManager);
    }
}