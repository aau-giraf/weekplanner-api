using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Giraf.IntegrationTests.Utils;
using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Data;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Organizations.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Giraf.IntegrationTests.Endpoints
{
    public class OrganizationEndpointsTests
    {
        #region Get Organizations for User Tests

        // Test GET /organizations/user/{id} when user has organizations
        [Fact]
        public async Task GetOrganizationsForUser_ReturnsListOfOrganizations()
        {
            // Arrange
            var seeder = new UserWithOrganizationsSeeder();
            var factory = new GirafWebApplicationFactory(seeder);
            var client = factory.CreateClient();

            // Retrieve the actual user ID from the seeded data
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var user = await dbContext.Users
                .Include(u => u.Organizations)
                .FirstOrDefaultAsync();
            Assert.NotNull(user);

            // Act
            var response = await client.GetAsync($"/organizations/user/{user.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var organizations = await response.Content.ReadFromJsonAsync<List<OrganizationNameOnlyDTO>>();
            Assert.NotNull(organizations);
            Assert.Equal(2, organizations.Count);
        }

        // Test GET /organizations/user/{id} when user does not exist
        [Fact]
        public async Task GetOrganizationsForUser_ReturnsBadRequest_WhenUserDoesNotExist()
        {
            // Arrange
            var seeder = new EmptyDb();
            var factory = new GirafWebApplicationFactory(seeder);
            var client = factory.CreateClient();
            var nonExistentUserId = "nonexistent_user_id";

            // Act
            var response = await client.GetAsync($"/organizations/user/{nonExistentUserId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Get Organization by ID Tests

        // Test GET /organizations/{id} when the organization exists
        [Fact]
        public async Task GetOrganizationById_ReturnsOrganization_WhenOrganizationExists()
        {
            // Arrange
            var seeder = new BasicOrganizationSeeder();
            var factory = new GirafWebApplicationFactory(seeder);
            var client = factory.CreateClient();

            int organizationId;

            // Retrieve the organization ID after the database is seeded
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var organization = await dbContext.Organizations.FirstOrDefaultAsync();
                Assert.NotNull(organization);
                organizationId = organization.Id;
            }

            // Act
            var response = await client.GetAsync($"/organizations/{organizationId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var organizationDto = await response.Content.ReadFromJsonAsync<OrganizationDTO>();
            Assert.NotNull(organizationDto);
            Assert.Equal(organizationId, organizationDto.Id);
        }

        // Test GET /organizations/{id} when the organization does not exist
        [Fact]
        public async Task GetOrganizationById_ReturnsNotFound_WhenOrganizationDoesNotExist()
        {
            // Arrange
            var seeder = new EmptyDb();
            var factory = new GirafWebApplicationFactory(seeder);
            var client = factory.CreateClient();
            var nonExistentOrganizationId = 9999;

            // Act
            var response = await client.GetAsync($"/organizations/{nonExistentOrganizationId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region Create Organization Tests

        // Test POST /organizations to create a new organization
        [Fact]
        public async Task PostOrganization_ReturnsCreated_WhenUserIsValid()
        {
            // Arrange
            var seeder = new BasicUserSeeder();
            var factory = new GirafWebApplicationFactory(seeder);
            var client = factory.CreateClient();

            // Retrieve the seeded user's ID from the database
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();

            var user = await dbContext.Users.FirstOrDefaultAsync();
            Assert.NotNull(user);

            var newOrgDto = new CreateOrganizationDTO { Name = "New Organization" };

            // Act
            var response = await client.PostAsJsonAsync($"/organizations?id={user.Id}", newOrgDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var createdOrganization = await response.Content.ReadFromJsonAsync<OrganizationDTO>();
            Assert.NotNull(createdOrganization);
            Assert.Equal("New Organization", createdOrganization.Name);
        }

        // Test POST /organizations when user does not exist
        [Fact]
        public async Task PostOrganization_ReturnsBadRequest_WhenUserDoesNotExist()
        {
            // Arrange
            var seeder = new EmptyDb();
            var factory = new GirafWebApplicationFactory(seeder);
            var client = factory.CreateClient();
            var nonExistentUserId = "nonexistent_user_id";
            var newOrgDto = new CreateOrganizationDTO { Name = "Another Organization" };

            // Act
            var response = await client.PostAsJsonAsync($"/organizations?id={nonExistentUserId}", newOrgDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Change Organization Name Tests

        // Test PUT /organizations/{id}/change-name when the organization exists
        [Fact]
        public async Task ChangeOrganizationName_ReturnsOk_WhenOrganizationExists()
        {
            // Arrange
            var seeder = new BasicOrganizationSeeder();
            var factory = new GirafWebApplicationFactory(seeder);
            var client = factory.CreateClient();

            int organizationId;

            // Retrieve the organization ID after the database is seeded
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var organization = await dbContext.Organizations.FirstOrDefaultAsync();
                Assert.NotNull(organization);

                organizationId = organization.Id;
            }

            var newName = "Updated Organization Name";

            // Act
            var response = await client.PutAsync($"/organizations/{organizationId}/change-name?newName={newName}", null);

            // Assert
            response.EnsureSuccessStatusCode();
            var updatedOrganization = await response.Content.ReadFromJsonAsync<OrganizationNameOnlyDTO>();
            Assert.NotNull(updatedOrganization);
            Assert.Equal(newName, updatedOrganization.Name);
        }

        // Test PUT /organizations/{id}/change-name when organization does not exist
        [Fact]
        public async Task ChangeOrganizationName_ReturnsNotFound_WhenOrganizationDoesNotExist()
        {
            // Arrange
            var seeder = new EmptyDb();
            var factory = new GirafWebApplicationFactory(seeder);
            var client = factory.CreateClient();
            var nonExistentOrgId = 9999;
            var newName = "Nonexistent Organization Name";

            // Act
            var response = await client.PutAsync($"/organizations/{nonExistentOrgId}/change-name?newName={newName}", null);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region Delete Organization Tests

        // Test DELETE /organizations/{id} when the organization exists
        [Fact]
        public async Task DeleteOrganization_ReturnsNoContent_WhenOrganizationExists()
        {
            // Arrange
            var seeder = new BasicOrganizationSeeder();
            var factory = new GirafWebApplicationFactory(seeder);
            var client = factory.CreateClient();

            int organizationId;

            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var organization = await dbContext.Organizations.FirstOrDefaultAsync();
                Assert.NotNull(organization);
                organizationId = organization.Id;
            }

            // Act
            var response = await client.DeleteAsync($"/organizations/{organizationId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify that the organization was deleted from the database
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var deletedOrganization = await dbContext.Organizations.FindAsync(organizationId);
                Assert.Null(deletedOrganization);
            }
        }

        // Test DELETE /organizations/{id} when the organization does not exist
        [Fact]
        public async Task DeleteOrganization_ReturnsNotFound_WhenOrganizationDoesNotExist()
        {
            // Arrange
            var seeder = new EmptyDb();
            var factory = new GirafWebApplicationFactory(seeder);
            var client = factory.CreateClient();
            var nonExistentOrgId = 9999;

            // Act
            var response = await client.DeleteAsync($"/organizations/{nonExistentOrgId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region Remove User from Organization Tests

        // Test PUT /organizations/{id}/remove-user/{userId} when the organization and user exist
        [Fact]
        public async Task RemoveUser_ReturnsOk_WhenOrganizationAndUserExist()
        {
            // Arrange
            var seeder = new OrganizationWithUserSeeder();
            var factory = new GirafWebApplicationFactory(seeder);
            var client = factory.CreateClient();

            int organizationId;
            string userId;

            // Retrieve the organization and user IDs after the database is seeded
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var organization = await dbContext.Organizations
                    .Include(o => o.Users)
                    .FirstOrDefaultAsync();
                Assert.NotNull(organization);

                organizationId = organization.Id;

                var user = organization.Users.FirstOrDefault();
                Assert.NotNull(user);

                userId = user.Id;
            }

            // Act
            var response = await client.PutAsync($"/organizations/{organizationId}/remove-user/{userId}", null);

            // Assert
            response.EnsureSuccessStatusCode();
            var updatedOrganization = await response.Content.ReadFromJsonAsync<OrganizationDTO>();
            Assert.NotNull(updatedOrganization);
            Assert.DoesNotContain(userId, updatedOrganization.Users.Select(u => u.Id));
        }

        // Test PUT /organizations/{id}/remove-user/{userId} when the user does not exist
        [Fact]
        public async Task RemoveUser_ReturnsBadRequest_WhenUserDoesNotExist()
        {
            // Arrange
            var seeder = new BasicOrganizationSeeder();
            var factory = new GirafWebApplicationFactory(seeder);
            var client = factory.CreateClient();

            int organizationId;

            // Retrieve the organization ID after the database is seeded
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var organization = await dbContext.Organizations.FirstOrDefaultAsync();
                Assert.NotNull(organization);

                organizationId = organization.Id;
            }

            var nonExistentUserId = "nonexistent_user_id";

            // Act
            var response = await client.PutAsync($"/organizations/{organizationId}/remove-user/{nonExistentUserId}", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test PUT /organizations/{id}/remove-user/{userId} when the organization does not exist
        [Fact]
        public async Task RemoveUser_ReturnsBadRequest_WhenOrganizationDoesNotExist()
        {
            // Arrange
            var seeder = new BasicUserSeeder();
            var factory = new GirafWebApplicationFactory(seeder);
            var client = factory.CreateClient();

            // Retrieve the seeded user from the database
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync();
            Assert.NotNull(user);

            var nonExistentOrgId = 9999; // Using an ID that doesn't exist in the database
            var userId = user.Id;

            // Act
            var response = await client.PutAsync($"/organizations/{nonExistentOrgId}/remove-user/{userId}", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion
    }
}