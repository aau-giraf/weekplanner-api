# Testing Environment Setup Guide

This guide aims to help you understand how the testing environment is set up for the Giraf project. It is designed to assist university students or developers who are unfamiliar with the frameworks and the structure of the program. By the end of this guide, you should have a clear understanding of how the testing environment works, how the seeders populate the database, and how they are used in the endpoint testing files.

## Table of Contents
- [Introduction](#introduction)
- [GirafWebApplicationFactory](#girafwebapplicationfactory)
- [Base Seeder (DbSeeder)](#base-seeder-dbseeder)
- [Empty Database Seeder (EmptyDb)](#empty-database-seeder-emptydb)
- [Basic Seeder Files](#basic-seeder-files)
  - [Example: BasicCitizenSeeder](#example-basiccitizenseeder)
- [Using Seeders in Endpoint Tests](#using-seeders-in-endpoint-tests)
- [Conclusion](#conclusion)

## Introduction

The testing environment is a crucial part of the Giraf project, allowing for automated integration tests that ensure the application's endpoints behave as expected. The testing setup utilizes:

- ASP.NET Core's WebApplicationFactory to create a test server.
- SQLite in-memory databases to simulate the application's database in a controlled environment.
- Seeders to populate the test database with specific data scenarios required for each test.

This setup allows tests to run in isolation, with a fresh database state for each test, ensuring that tests do not interfere with each other and that results are consistent.

## GirafWebApplicationFactory

The GirafWebApplicationFactory is a custom factory that extends `WebApplicationFactory<Program>`. It configures the test server and database for each test.

```csharp
using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Giraf.IntegrationTests.Utils;

// This factory creates a Giraf web API configured for testing.
internal class GirafWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly DbSeeder _seeder;

    public GirafWebApplicationFactory(DbSeeder seeder)
    {
        _seeder = seeder;
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

            // Build the service provider and create a scope
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();

            // Clear the database before seeding
            dbContext.Database.EnsureDeleted();

            // Use migrations to apply schema, especially for identity tables
            dbContext.Database.Migrate();

            // Seed the database with scenario-specific data
            _seeder.SeedData(dbContext);
        });
    }
}
```

### How It Works
- **Environment Setup**: Sets the environment to "Testing" to use test-specific configurations.
- **Service Configuration**: Removes existing DbContext configurations to replace them with test-specific ones.
- **Database Configuration**:
  - Uses SQLite with a unique database file for each test to avoid conflicts.
  - Ensures the database is deleted before each test to start with a clean slate.
  - Applies migrations to set up the schema, including identity tables.
- **Seeding Data**: Uses a DbSeeder to populate the database with the required data for the test scenario.

### Purpose
The factory provides a fully configured test server with a fresh database state for each test. By injecting different seeders, tests can simulate various data scenarios and edge cases.

## Base Seeder (DbSeeder)

The `DbSeeder` is an abstract base class that defines the structure for all seeders used in the tests.

```csharp
using Microsoft.EntityFrameworkCore;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

public abstract class DbSeeder
{
    public abstract void SeedData(DbContext context);
}
```

### How It Works
- **Abstract Class**: Cannot be instantiated directly. It serves as a blueprint for specific seeders.
- **SeedData Method**: Must be implemented by derived classes to populate the database with specific data.

### Purpose
The base seeder ensures consistency across different seeders by enforcing the implementation of the `SeedData` method. It allows for flexible and reusable data seeding tailored to each test's requirements.

## Empty Database Seeder (EmptyDb)

The `EmptyDb` seeder is a simple implementation of the `DbSeeder` that leaves the database empty.

```csharp
using Microsoft.EntityFrameworkCore;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

public class EmptyDb : DbSeeder
{
    public override void SeedData(DbContext context)
    {
        // No data is seeded; the database remains empty.
    }
}
```

### How It Works
- **No Implementation**: The `SeedData` method is intentionally left empty.
- **Purpose in Tests**: Used when a test requires an empty database, such as testing behavior when no data exists.

### Purpose
Provides a clean database state without any data, allowing tests to verify how the application handles scenarios where expected data is missing.

## Basic Seeder Files

Basic seeder files extend the `DbSeeder` base class to populate the database with specific entities needed for tests. They are tailored to set up minimal data required for a test case.

### Example: BasicCitizenSeeder

```csharp
using Microsoft.EntityFrameworkCore;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Activities;
using GirafAPI.Data;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using GirafAPI.Entities.Grades;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

// Seeder to add a single citizen for basic testing
public class BasicCitizenSeeder : DbSeeder
{
    public override void SeedData(DbContext context)
    {
        var dbContext = (GirafDbContext)context;

        var organization = new Organization
        {
            Name = "Test Organization for Citizen",
            Users = new List<GirafUser>(),
            Citizens = new List<Citizen>(),
            Grades = new List<Grade>()
        };

        // Add an organization to the context
        dbContext.Organizations.Add(organization);
        dbContext.SaveChanges();

        dbContext.Citizens.Add(new Citizen
        {
            FirstName = "Anders",
            LastName = "And",
            Organization = organization!,
            Activities = new List<Activity>()
        });

        dbContext.SaveChanges();
    }
}
```

### How It Works
- **Casting DbContext**: The `DbContext` is cast to `GirafDbContext` to access specific `DbSet` properties.
- **Creating Entities**:
  - **Organization**: An organization is created as it's required for the citizen.
  - **Citizen**: A citizen is created and associated with the organization.
- **Adding to Context**: Entities are added to the `DbContext` and saved to the database.
- **Purpose in Tests**: Provides a basic setup with a single citizen and organization for tests that require at least one citizen in the database.

### Purpose
Allows tests to focus on functionality involving citizens without the overhead of setting up the entire database. It ensures the necessary data exists for the test to run successfully.

## Using Seeders in Endpoint Tests

Seeders are used in endpoint tests to set up the required data before each test runs. They provide a controlled environment where specific scenarios can be tested.

### Example Usage in a Test File

```csharp
using Xunit;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Json;
using Giraf.IntegrationTests.Utils;
using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Entities.Citizens.DTOs;

namespace Giraf.IntegrationTests.Endpoints
{
    public class CitizenEndpointsTests
    {
        [Fact]
        public async Task GetCitizenById_ReturnsCitizen_WhenCitizenExists()
        {
            // Arrange
            var seeder = new BasicCitizenSeeder();
            var factory = new GirafWebApplicationFactory(seeder);
            var client = factory.CreateClient();

            int citizenId;
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var citizen = await dbContext.Citizens.FirstOrDefaultAsync();
                Assert.NotNull(citizen);
                citizenId = citizen.Id;
            }

            // Act
            var response = await client.GetAsync($"/citizens/{citizenId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var citizenDto = await response.Content.ReadFromJsonAsync<CitizenDTO>();
            Assert.NotNull(citizenDto);
            Assert.Equal(citizenId, citizenDto.Id);
        }
    }
}
```

### How It Works
- **Instantiate Seeder**: A specific seeder (`BasicCitizenSeeder`) is instantiated to set up the required data.
  ```csharp
  var seeder = new BasicCitizenSeeder();
  ```
- **Create Factory with Seeder**: The `GirafWebApplicationFactory` is created, passing the seeder as a parameter.
  ```csharp
  var factory = new GirafWebApplicationFactory(seeder);
  ```
- **Create Client**: An HTTP client is created from the factory to simulate requests to the API.
  ```csharp
  var client = factory.CreateClient();
  ```
- **Retrieve Data**: Within a scope, the test retrieves data from the database set up by the seeder.
  ```csharp
  using (var scope = factory.Services.CreateScope())
  {
      var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
      var citizen = await dbContext.Citizens.FirstOrDefaultAsync();
      Assert.NotNull(citizen);
      citizenId = citizen.Id;
  }
  ```
- **Act**: The test performs the HTTP request to the endpoint.
  ```csharp
  var response = await client.GetAsync($"/citizens/{citizenId}");
  ```
- **Assert**: The test checks the response to ensure the endpoint behaves as expected.
  ```csharp
  response.EnsureSuccessStatusCode();
  var citizenDto = await response.Content.ReadFromJsonAsync<CitizenDTO>();
  Assert.NotNull(citizenDto);
  Assert.Equal(citizenId, citizenDto.Id);
  ```

### Purpose
Using seeders in this way ensures that each test has full control over the database state. It allows tests to be repeatable and independent, as they don't rely on data from other tests.

## Conclusion

The testing environment in the Giraf project is designed to be flexible, reliable, and easy to understand. By using the `GirafWebApplicationFactory` and various seeders, tests can simulate a wide range of scenarios with precise control over the database state.

### Key Takeaways
- **Factory Pattern**: The custom factory sets up a test server with a fresh database for each test.
- **Seeders**: Provide a way to populate the database with specific data needed for tests.
- **Isolation**: Each test runs in isolation, ensuring consistent results and no interference between tests.
- **Reusability**: Seeders and the factory can be reused across multiple tests, promoting DRY (Don't Repeat Yourself) principles.