using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Giraf.IntegrationTests.Utils;
using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Data;
using GirafAPI.Entities.Pictograms.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Giraf.IntegrationTests.Endpoints
{
    [Collection("IntegrationTests")]
    public class PictogramEndpointsTests
    {
        #region Create Pictogram Tests

        [Fact]
        public async Task CreatePictogram_ReturnsOk_WhenPictogramIsValid()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            int organizationId = seeder.Organizations[0].Id;

            // Prepare multipart form data
            var formData = new MultipartFormDataContent();

            // Add image file
            var imageContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(imageContent, "image", "test.png");

            // Add organizationId and pictogramName
            formData.Add(new StringContent(organizationId.ToString()), "organizationId");
            formData.Add(new StringContent("TestPictogram"), "pictogramName");

            // Act
            var response = await client.PostAsync($"/pictograms/{organizationId}", formData);

            // Assert
            response.EnsureSuccessStatusCode();
            var pictogramUrl = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(pictogramUrl));

            // Verify that the pictogram was saved in the database
            using (var verificationScope = factory.Services.CreateScope())
            {
                var context = verificationScope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var pictogram = await context.Pictograms.FirstOrDefaultAsync(p => p.PictogramName == "TestPictogram");
                Assert.NotNull(pictogram);
                Assert.Equal(organizationId, pictogram.OrganizationId);
            }
        }

        [Fact]
        public async Task CreatePictogram_ReturnsBadRequest_WhenImageIsMissing()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            // Prepare multipart form data without image
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("1"), "organizationId");
            formData.Add(new StringContent("TestPictogram"), "pictogramName");
            
            var organizatonId = seeder.Organizations[0].Id;

            // Act
            var response = await client.PostAsync($"/pictograms/{organizatonId}", formData);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreatePictogram_ReturnsNotFound_WhenOrganizationIdIsInvalid()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            // Prepare multipart form data without organizationId
            var formData = new MultipartFormDataContent();

            // Add image file
            var imageContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(imageContent, "image", "test.png");

            // Add pictogramName
            formData.Add(new StringContent("TestPictogram"), "pictogramName");
            
            var organizatonId = 999;

            // Act
            var response = await client.PostAsync($"/pictograms/{organizatonId}", formData);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreatePictogram_ReturnsBadRequest_WhenPictogramNameIsMissing()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            int organizationId = seeder.Organizations[0].Id;

            // Prepare multipart form data without pictogramName
            var formData = new MultipartFormDataContent();

            // Add image file
            var imageContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(imageContent, "image", "test.png");

            // Add organizationId
            formData.Add(new StringContent(organizationId.ToString()), "organizationId");

            // Act
            var response = await client.PostAsync($"/pictograms/{organizationId}", formData);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Get Pictogram by ID Tests

        [Fact]
        public async Task GetPictogramById_ReturnsPictogram_WhenPictogramExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            int pictogramId = seeder.Pictograms[0].Id;

            var organizationId = seeder.Organizations[0].Id;

            // Act
            var response = await client.GetAsync($"/pictograms/{organizationId}/{pictogramId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var pictogramDto = await response.Content.ReadFromJsonAsync<PictogramDTO>();
            Assert.NotNull(pictogramDto);
            Assert.Equal(pictogramId, pictogramDto.Id);
        }

        [Fact]
        public async Task GetPictogramById_ReturnsNotFound_WhenPictogramDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);
            
            int nonExistentPictogramId = 9999;
            
            var organizationId = seeder.Organizations[0].Id;

            // Act
            var response = await client.GetAsync($"/pictograms/{organizationId}/{nonExistentPictogramId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region Get Pictograms by Organization ID Tests

        [Fact]
        public async Task GetPictogramsByOrganizationId_ReturnsPictograms_WhenPictogramsExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            int organizationId = seeder.Organizations[0].Id;

            var currentPage = 1;
            var pageSize = 10;

            // Act
            var response = await client.GetAsync($"/pictograms/{organizationId}?currentPage={currentPage}&pageSize={pageSize}");

            // Assert
            response.EnsureSuccessStatusCode();
            var pictograms = await response.Content.ReadFromJsonAsync<List<PictogramDTO>>();
            Assert.NotNull(pictograms);
            Assert.NotEmpty(pictograms);
        }

        [Fact]
        public async Task GetPictogramsByOrganizationId_ReturnsEmptyList_WhenNoPictogramsExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            int organizationId = seeder.Organizations[0].Id;
            
            var currentPage = 1;
            var pageSize = 10;

            // Act
            var response = await client.GetAsync($"/pictograms/{organizationId}?currentPage={currentPage}&pageSize={pageSize}");

            // Assert
            response.EnsureSuccessStatusCode();
            var pictograms = await response.Content.ReadFromJsonAsync<List<PictogramDTO>>();
            Assert.NotNull(pictograms);
            Assert.Empty(pictograms);
        }

        #endregion

        #region Delete Pictogram Tests

        [Fact]
        public async Task DeletePictogram_ReturnsOk_WhenPictogramExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["owner"]);

            var organizationId = seeder.Organizations[0].Id;
            int pictogramId = seeder.Pictograms[0].Id;

            // Act
            var response = await client.DeleteAsync($"/pictograms/{organizationId}/{pictogramId}");

            // Assert
            response.EnsureSuccessStatusCode();

            // Verify that the pictogram was deleted
            using (var verificationScope = factory.Services.CreateScope())
            {
                var context = verificationScope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var deletedPictogram = await context.Pictograms.FindAsync(pictogramId);
                Assert.Null(deletedPictogram);
            }
        }

        [Fact]
        public async Task DeletePictogram_ReturnsNotFound_WhenPictogramDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new OnlyUsersAndOrgDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["owner"]);

            var organizationId = seeder.Organizations[0].Id;
            int nonExistentPictogramId = 9999;

            // Act
            var response = await client.DeleteAsync($"/pictograms/{organizationId}/{nonExistentPictogramId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion
    }
}
