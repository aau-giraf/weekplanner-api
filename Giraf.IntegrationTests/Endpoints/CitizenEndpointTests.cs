using System.Net;
using System.Net.Http.Json;
using Giraf.IntegrationTests.Utils;
using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Data;
using GirafAPI.Entities.Citizens.DTOs;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Giraf.IntegrationTests.Endpoints
{
    [Collection("IntegrationTests")]
    public class CitizenEndpointTests
    {
        #region Get All Citizens Tests

        // 1. Testing GET /citizens, with multiple existing citizens in the DB
        [Fact]
        public async Task GetAllCitizens_ReturnsListOfCitizens()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["admin"]);

            // Act
            var response = await client.GetAsync("/citizens");

            // Assert
            response.EnsureSuccessStatusCode();
            var citizens = await response.Content.ReadFromJsonAsync<List<CitizenDTO>>();
            Assert.NotNull(citizens);
            Assert.Equal(3, citizens.Count);
        }

        // 2. Test GET /citizens when there are no citizens.
        [Fact]
        public async Task GetAllCitizens_ReturnsEmptyList_WhenNoCitizens()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            // Act
            var response = await client.GetAsync("/citizens");

            // Assert
            response.EnsureSuccessStatusCode();
            var citizens = await response.Content.ReadFromJsonAsync<List<CitizenDTO>>();
            Assert.NotNull(citizens);
            Assert.Empty(citizens);
        }

        #endregion

        #region Get Citizen by ID Tests

        // 3. Test GET /citizens/{id:int} when the citizen exists.
        [Fact]
        public async Task GetCitizenById_ReturnsCitizen_WhenCitizenExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["admin"]);

            var citizenId = seeder.Citizens[0].Id;

            // Act
            var response = await client.GetAsync($"/citizens/{citizenId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var citizen = await response.Content.ReadFromJsonAsync<CitizenDTO>();
            Assert.NotNull(citizen);
            Assert.Equal(citizenId, citizen.Id);
            Assert.Equal("Anders", citizen.FirstName);
            Assert.Equal("And", citizen.LastName);
        }

        // 4. Test GET /citizens/{id:int} when the citizen does not exist.
        [Fact]
        public async Task GetCitizenById_ReturnsNotFound_WhenCitizenDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            // Act
            var response = await client.GetAsync("/citizens/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region Update Citizen Tests

        // 5. Test PUT /citizens/{id:int} when updating an existing citizen.
        [Fact]
        public async Task UpdateCitizen_ReturnsOk_WhenCitizenExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["admin"]);

            var citizenId = seeder.Citizens[0].Id;

            var updateCitizenDto = new UpdateCitizenDTO("UpdatedFirstName", "UpdatedLastName");

            // Act
            var response = await client.PutAsJsonAsync($"/citizens/{citizenId}", updateCitizenDto);

            // Assert
            response.EnsureSuccessStatusCode();

            // Verify that the citizen was updated
            var getResponse = await client.GetAsync($"/citizens/{citizenId}");
            getResponse.EnsureSuccessStatusCode();
            var updatedCitizen = await getResponse.Content.ReadFromJsonAsync<CitizenDTO>();
            Assert.NotNull(updatedCitizen);
            Assert.Equal("UpdatedFirstName", updatedCitizen.FirstName);
            Assert.Equal("UpdatedLastName", updatedCitizen.LastName);
        }

        // 6. Test PUT /citizens/{id:int} when the citizen does not exist.
        [Fact]
        public async Task UpdateCitizen_ReturnsNotFound_WhenCitizenDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            var updateCitizenDto = new UpdateCitizenDTO("FirstName", "LastName");

            // Act
            var response = await client.PutAsJsonAsync("/citizens/999", updateCitizenDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region Add Citizen to Organization Tests

        // 7. Test POST /citizens/{id}/add-citizen when the organization exists.
        [Fact]
        public async Task AddCitizen_ReturnsOk_WhenOrganizationExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            
            client.AttachClaimsToken(scope, seeder.Users["admin"]);

            var createCitizenDto = new CreateCitizenDTO("New", "Citizen");
            var organizationId = seeder.Organizations.First().Id;

            // Act
            var response = await client.PostAsJsonAsync($"/citizens/{organizationId}/add-citizen", createCitizenDto);

            // Assert
            response.EnsureSuccessStatusCode();

            // Verify that the citizen was added
            var getCitizensResponse = await client.GetAsync("/citizens");
            getCitizensResponse.EnsureSuccessStatusCode();
            var citizens = await getCitizensResponse.Content.ReadFromJsonAsync<List<CitizenDTO>>();
            Assert.NotNull(citizens);
            Assert.Equal("New", citizens[3].FirstName);
            Assert.Equal("Citizen", citizens[3].LastName);
        }

        // 8. Test POST /citizens/{id}/add-citizen when the organization does not exist.
        [Fact]
        public async Task AddCitizen_ReturnsNotFound_WhenOrganizationDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            seeder.SeedUsers(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            var client = factory.CreateClient();
            
            client.AttachClaimsToken(scope, seeder.Users["admin"]);

            var createCitizenDto = new CreateCitizenDTO("New", "Citizen");

            // Act
            var response = await client.PostAsJsonAsync("/citizens/999/add-citizen", createCitizenDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region Remove Citizen from Organization Tests

        // 9. Test DELETE /citizens/{id}/remove-citizen/{citizenId} when the citizen exists in the organization.
        [Fact]
        public async Task RemoveCitizen_ReturnsNoContent_WhenCitizenExistsInOrganization()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["admin"]);

            // Get the organization ID and citizen ID
            var organizationId = seeder.Organizations.First().Id;
            
            var citizenId = seeder.Citizens[0].Id;

            // Act
            var response = await client.DeleteAsync($"/citizens/{organizationId}/remove-citizen/{citizenId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify that the citizen was removed
            var getCitizensResponse = await client.GetAsync("/citizens");
            getCitizensResponse.EnsureSuccessStatusCode();
            var citizens = await getCitizensResponse.Content.ReadFromJsonAsync<List<CitizenDTO>>();
            Assert.NotNull(citizens);
            Assert.Equal(2, citizens.Count);
        }

        // 10. Test DELETE /citizens/{id}/remove-citizen/{citizenId} when the citizen does not exist.
        [Fact]
        public async Task RemoveCitizen_ReturnsNotFound_WhenCitizenDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["admin"]);
                
            var organizationId = seeder.Organizations.First().Id;

            // Act
            var response = await client.DeleteAsync($"/citizens/{organizationId}/remove-citizen/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // 11. Test DELETE /citizens/{id}/remove-citizen/{citizenId} when the citizen does not belong to the organization.
        [Fact]
        public async Task RemoveCitizen_ReturnsBadRequest_WhenCitizenNotInOrganization()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedOrganization(
                scope.ServiceProvider.GetRequiredService<GirafDbContext>(),
                scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>(),
                seeder.Users["owner"],
                new List<GirafUser>(),
                new List<GirafUser>()
            );
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["owner"]);
            
            var organizationId = seeder.Organizations[1].Id;
            var citizenId = seeder.Citizens[0].Id;

            // Act
            var response = await client.DeleteAsync($"/citizens/{organizationId}/remove-citizen/{citizenId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion
    }
}
