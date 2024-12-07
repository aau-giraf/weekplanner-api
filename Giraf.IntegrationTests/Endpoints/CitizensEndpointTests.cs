using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Giraf.IntegrationTests.Utils;
using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Data;
using GirafAPI.Entities.Citizens.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Giraf.IntegrationTests.Endpoints
{
    [Collection("IntegrationTests")]
    public class CitizensEndpointTests
    {
        
        #region Get All Citizens Tests

        // 1. Testing GET /citizens, with multiple existing citizens in the DB
        [Fact]
        public async Task GetAllCitizens_ReturnsListOfCitizens()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(_ => new MultipleCitizensSeeder());
            var client = factory.CreateClient();

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
            var factory = new GirafWebApplicationFactory(_ => new EmptyDb());
            var client = factory.CreateClient();

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
            var factory = new GirafWebApplicationFactory(_ => new BasicCitizenSeeder());
            var client = factory.CreateClient();

            // First, get the list of citizens to obtain the ID
            var citizensResponse = await client.GetAsync("/citizens");
            citizensResponse.EnsureSuccessStatusCode();
            var citizens = await citizensResponse.Content.ReadFromJsonAsync<List<CitizenDTO>>();
            Assert.NotNull(citizens);

            var citizenId = citizens[0].Id;

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
            var factory = new GirafWebApplicationFactory(_ => new EmptyDb());
            var client = factory.CreateClient();

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
            var factory = new GirafWebApplicationFactory(_ => new BasicCitizenSeeder());
            var client = factory.CreateClient();

            // Get the citizen's ID
            var citizensResponse = await client.GetAsync("/citizens");
            citizensResponse.EnsureSuccessStatusCode();
            var citizens = await citizensResponse.Content.ReadFromJsonAsync<List<CitizenDTO>>();
            Assert.NotNull(citizens);
            Assert.Single(citizens);

            var citizenId = citizens[0].Id;

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
            var factory = new GirafWebApplicationFactory(_ => new EmptyDb());
            var client = factory.CreateClient();

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
            var factory = new GirafWebApplicationFactory(_ => new BasicOrganizationSeeder());
            var client = factory.CreateClient();

            // Get the organization ID
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var organization = await dbContext.Organizations.FirstOrDefaultAsync();
                Assert.NotNull(organization);
                var organizationId = organization.Id;

                var createCitizenDto = new CreateCitizenDTO("New", "Citizen");

                // Act
                var response = await client.PostAsJsonAsync($"/citizens/{organizationId}/add-citizen", createCitizenDto);

                // Assert
                response.EnsureSuccessStatusCode();

                // Verify that the citizen was added
                var getCitizensResponse = await client.GetAsync("/citizens");
                getCitizensResponse.EnsureSuccessStatusCode();
                var citizens = await getCitizensResponse.Content.ReadFromJsonAsync<List<CitizenDTO>>();
                Assert.NotNull(citizens);
                Assert.Single(citizens);
                Assert.Equal("New", citizens[0].FirstName);
                Assert.Equal("Citizen", citizens[0].LastName);
            }
        }

        // 8. Test POST /citizens/{id}/add-citizen when the organization does not exist.
        [Fact]
        public async Task AddCitizen_ReturnsNotFound_WhenOrganizationDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(_ => new EmptyDb());
            var client = factory.CreateClient();

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
            var factory = new GirafWebApplicationFactory(_ => new CitizenWithOrganizationSeeder());
            var client = factory.CreateClient();

            // Get the organization ID and citizen ID
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var organization = await dbContext.Organizations.FirstOrDefaultAsync();
                Assert.NotNull(organization);
                var organizationId = organization.Id;

                var citizen = await dbContext.Citizens.FirstOrDefaultAsync();
                Assert.NotNull(citizen);
                var citizenId = citizen.Id;

                // Act
                var response = await client.DeleteAsync($"/citizens/{organizationId}/remove-citizen/{citizenId}");

                // Assert
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                // Verify that the citizen was removed
                var getCitizensResponse = await client.GetAsync("/citizens");
                getCitizensResponse.EnsureSuccessStatusCode();
                var citizens = await getCitizensResponse.Content.ReadFromJsonAsync<List<CitizenDTO>>();
                Assert.NotNull(citizens);
                Assert.Empty(citizens);
            }
        }

        // 10. Test DELETE /citizens/{id}/remove-citizen/{citizenId} when the citizen does not exist.
        [Fact]
        public async Task RemoveCitizen_ReturnsNotFound_WhenCitizenDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(_ => new BasicOrganizationSeeder());
            var client = factory.CreateClient();

            // Get the organization ID
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var organization = await dbContext.Organizations.FirstOrDefaultAsync();
                Assert.NotNull(organization);
                var organizationId = organization.Id;

                // Act
                var response = await client.DeleteAsync($"/citizens/{organizationId}/remove-citizen/999");

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        // 11. Test DELETE /citizens/{id}/remove-citizen/{citizenId} when the citizen does not belong to the organization.
        [Fact]
        public async Task RemoveCitizen_ReturnsBadRequest_WhenCitizenNotInOrganization()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(_ => new MultipleOrganizationsAndCitizensSeeder());
            var client = factory.CreateClient();

            // Get the organization ID and a citizen ID from a different organization
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();

                var organizations = await dbContext.Organizations.ToListAsync();
                Assert.True(organizations.Count >= 2);

                var organization1 = organizations[0];
                var organization2 = organizations[1];

                var citizenNotInOrg = await dbContext.Citizens.FirstOrDefaultAsync(c => c.Organization.Id == organization2.Id);
                Assert.NotNull(citizenNotInOrg);
                var citizenId = citizenNotInOrg.Id;

                // Act
                var response = await client.DeleteAsync($"/citizens/{organization1.Id}/remove-citizen/{citizenId}");

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        #endregion
        
    }
}
