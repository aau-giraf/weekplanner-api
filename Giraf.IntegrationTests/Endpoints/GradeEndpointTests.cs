using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Giraf.IntegrationTests.Utils;
using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Data;
using GirafAPI.Entities.Grades.DTOs;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Giraf.IntegrationTests.Endpoints
{
    [Collection("IntegrationTests")]
    public class GradeEndpointsTests
    {
        #region 1. Get Grade by ID Tests

        // Test 1: Get a grade by ID when the grade exists.
        [Fact]
        public async Task GetGradeById_ReturnsGrade_WhenGradeExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>();
            var userClaims = await userManager.GetClaimsAsync(seeder.Users["member"]);
            TestAuthHandler.TestClaims = userClaims.ToList();
            
            int orgId = seeder.Organizations[0].Id;

            int gradeId = seeder.Grades[0].Id;

            // Act
            var response = await client.GetAsync($"/grades/{orgId}/{gradeId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var gradeDto = await response.Content.ReadFromJsonAsync<GradeDTO>();
            Assert.NotNull(gradeDto);
            Assert.Equal(gradeId, gradeDto.Id);
        }

        // Test 2: Get a grade by ID when the grade does not exist.
        [Fact]
        public async Task GetGradeById_ReturnsNotFound_WhenGradeDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>();
            var userClaims = await userManager.GetClaimsAsync(seeder.Users["member"]);
            TestAuthHandler.TestClaims = userClaims.ToList();
            
            int nonExistentGradeId = 9999;
            var testOrgId = seeder.Organizations[0].Id;

            // Act
            var response = await client.GetAsync($"/grades/{testOrgId}/{nonExistentGradeId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region 2. Get Grades in Organization Tests

        // Test 3: Get all grades within an organization when the organization exists.
        [Fact]
        public async Task GetGradesInOrganization_ReturnsGrades_WhenOrganizationExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>();
            var userClaims = await userManager.GetClaimsAsync(seeder.Users["member"]);
            TestAuthHandler.TestClaims = userClaims.ToList();

            int organizationId = seeder.Organizations[0].Id;

            // Act
            var response = await client.GetAsync($"/grades/org/{organizationId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var grades = await response.Content.ReadFromJsonAsync<List<GradeDTO>>();
            Assert.NotNull(grades);
            Assert.NotEmpty(grades);
        }

        // Test 4: Get all grades within an organization when the organization does not exist.
        [Fact]
        public async Task GetGradesInOrganization_ReturnsNotFound_WhenOrganizationDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>();
            var userClaims = await userManager.GetClaimsAsync(seeder.Users["member"]);
            TestAuthHandler.TestClaims = userClaims.ToList();
            
            int nonExistentOrganizationId = 9999;

            // Act
            var response = await client.GetAsync($"/grades/org/{nonExistentOrganizationId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region 3. Create Grade Tests

        // Test 5: Create a new grade when the organization exists.
        [Fact]
        public async Task CreateGrade_ReturnsCreated_WhenOrganizationExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>();
            var userClaims = await userManager.GetClaimsAsync(seeder.Users["member"]);
            TestAuthHandler.TestClaims = userClaims.ToList();

            int organizationId = seeder.Organizations[0].Id;

            var newGradeDto = new CreateGradeDTO
            (
                Name : "New Grade"
            );

            // Act
            var response = await client.PostAsJsonAsync($"/grades/{organizationId}", newGradeDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var gradeDto = await response.Content.ReadFromJsonAsync<GradeDTO>();
            Assert.NotNull(gradeDto);
            Assert.Equal(newGradeDto.Name, gradeDto.Name);
        }

        // Test 6: Create a new grade when the organization does not exist.
        [Fact]
        public async Task CreateGrade_ReturnsForbidden_WhenOrganizationDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            seeder.SeedUsers(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            var client = factory.CreateClient();
            
            TestAuthHandler.SetTestClaims(scope, seeder.Users["member"]);
            
            int nonExistentOrganizationId = 9999;

            var newGradeDto = new CreateGradeDTO
            (
                Name : "New Grade"
            );

            // Act
            var response = await client.PostAsJsonAsync($"/grades/{nonExistentOrganizationId}", newGradeDto);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        #endregion

        #region 4. Change Grade Name Tests

        // Test 7: Change the name of a grade when the grade exists.
        [Fact]
        public async Task ChangeGradeName_ReturnsOk_WhenGradeExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            TestAuthHandler.SetTestClaims(scope, seeder.Users["member"]);
            
            var orgId = seeder.Organizations[0].Id;
            int gradeId = seeder.Grades[0].Id;

            string newName = "Updated Grade Name";

            // Act
            var response = await client.PutAsync($"/grades/{orgId}/{gradeId}/change-name?newName={newName}", null);

            // Assert
            response.EnsureSuccessStatusCode();
            var gradeDto = await response.Content.ReadFromJsonAsync<GradeDTO>();
            Assert.NotNull(gradeDto);
            Assert.Equal(newName, gradeDto.Name);
        }

        // Test 8: Change the name of a grade when the grade does not exist.
        [Fact]
        public async Task ChangeGradeName_ReturnsForbidden_WhenGradeDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            
            TestAuthHandler.SetTestClaims(scope, seeder.Users["member"]);
            
            var orgId = seeder.Organizations[0].Id;
            int nonExistentGradeId = 9999;
            string newName = "Updated Grade Name";

            // Act
            var response = await client.PutAsync($"/grades/{orgId}/{nonExistentGradeId}/change-name?newName={newName}", null);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        #endregion

        #region 5. Add Citizens to Grade Tests

        // Test 9: Add citizens to a grade when both the grade and citizens exist.
        [Fact]
        public async Task AddCitizensToGrade_ReturnsOk_WhenGradeAndCitizensExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            
            TestAuthHandler.SetTestClaims(scope, seeder.Users["member"]);
            
            var orgId = seeder.Organizations[0].Id;

            int gradeId = seeder.Grades[0].Id;
            List<int> citizenIds = seeder.Citizens.Select(c => c.Id).ToList();

            // Act
            var response = await client.PutAsJsonAsync($"/grades/{orgId}/{gradeId}/add-citizens", citizenIds);

            // Assert
            response.EnsureSuccessStatusCode();
            var gradeDto = await response.Content.ReadFromJsonAsync<GradeDTO>();
            Assert.NotNull(gradeDto);
            Assert.NotEmpty(gradeDto.Citizens);
            Assert.Equal(citizenIds.Count, gradeDto.Citizens.Count);
        }

        // Test 10: Add citizens to a grade when the grade does not exist.
        [Fact]
        public async Task AddCitizensToGrade_ReturnsNotFound_WhenGradeDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedCitizens(scope.ServiceProvider.GetRequiredService<GirafDbContext>(), seeder.Organizations[0]);
            var client = factory.CreateClient();
            
            TestAuthHandler.SetTestClaims(scope, seeder.Users["admin"]);
            
            var orgId = seeder.Organizations[0].Id;

            int nonExistentGradeId = 9999;
            List<int> citizenIds = seeder.Citizens.Select(c => c.Id).ToList();

            // Act
            var response = await client.PutAsJsonAsync($"/grades/{orgId}/{nonExistentGradeId}/add-citizens", citizenIds);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region 6. Remove Citizens from Grade Tests

        // Test 11: Remove citizens from a grade when both the grade and citizens exist.
        [Fact]
        public async Task RemoveCitizensFromGrade_ReturnsOk_WhenGradeAndCitizensExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            
            TestAuthHandler.SetTestClaims(scope, seeder.Users["member"]);
            
            var orgId = seeder.Organizations[0].Id;
            int gradeId = seeder.Grades[0].Id;
            List<int> citizenIds = seeder.Citizens.Select(c => c.Id).ToList();

            // Act
            var response = await client.PutAsJsonAsync($"/grades/{orgId}/{gradeId}/remove-citizens", citizenIds);

            // Assert
            response.EnsureSuccessStatusCode();
            var gradeDto = await response.Content.ReadFromJsonAsync<GradeDTO>();
            Assert.NotNull(gradeDto);
            Assert.Empty(gradeDto.Citizens);
        }

        // Test 12: Remove citizens from a grade when the grade does not exist.
        [Fact]
        public async Task RemoveCitizensFromGrade_ReturnsNotFound_WhenGradeDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            seeder.SeedCitizens(scope.ServiceProvider.GetRequiredService<GirafDbContext>(), seeder.Organizations[0]);
            var client = factory.CreateClient();
            
            TestAuthHandler.SetTestClaims(scope, seeder.Users["member"]);
            
            var orgId = seeder.Organizations[0].Id;
            int nonExistentGradeId = 9999;
            List<int> citizenIds = seeder.Citizens.Select(c => c.Id).ToList();

            // Act
            var response = await client.PutAsJsonAsync($"/grades/{orgId}/{nonExistentGradeId}/remove-citizens", citizenIds);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region 7. Delete Grade Tests

        // Test 13: Delete a grade when the grade exists.
        [Fact]
        public async Task DeleteGrade_ReturnsNoContent_WhenGradeExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            
            TestAuthHandler.SetTestClaims(scope, seeder.Users["member"]);

            int orgId = seeder.Organizations[0].Id;
            int gradeId = seeder.Grades[0].Id;

            // Act
            var response = await client.DeleteAsync($"/grades/{orgId}/{gradeId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify that the grade has been deleted
            using (var verificationScope = factory.Services.CreateScope())
            {
                var dbContext = verificationScope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var deletedGrade = await dbContext.Grades.FindAsync(gradeId);
                Assert.Null(deletedGrade);
            }
        }

        // Test 14: Delete a grade when the grade does not exist.
        [Fact]
        public async Task DeleteGrade_ReturnsForbidden_WhenGradeDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            
            TestAuthHandler.SetTestClaims(scope, seeder.Users["member"]);
            
            var orgId = seeder.Organizations[0].Id;
            int nonExistentGradeId = 9999;

            // Act
            var response = await client.DeleteAsync($"/grades/{orgId}/{nonExistentGradeId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        #endregion
    }
}
