using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Giraf.IntegrationTests.Utils;
using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Data;
using GirafAPI.Entities.Pictograms.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

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
            var factory = new GirafWebApplicationFactory(_ => new BasicOrganizationSeeder());
            var client = factory.CreateClient();

            int organizationId;

            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var organization = await context.Organizations.FirstOrDefaultAsync();
                Assert.NotNull(organization);
                organizationId = organization.Id;
            }

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
            var response = await client.PostAsync("/pictograms/", formData);

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
            var factory = new GirafWebApplicationFactory(_ => new BasicOrganizationSeeder());
            var client = factory.CreateClient();

            // Prepare multipart form data without image
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("1"), "organizationId");
            formData.Add(new StringContent("TestPictogram"), "pictogramName");

            // Act
            var response = await client.PostAsync("/pictograms/", formData);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreatePictogram_ReturnsBadRequest_WhenOrganizationIdIsMissing()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(_ => new EmptyDb());
            var client = factory.CreateClient();

            // Prepare multipart form data without organizationId
            var formData = new MultipartFormDataContent();

            // Add image file
            var imageContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(imageContent, "image", "test.png");

            // Add pictogramName
            formData.Add(new StringContent("TestPictogram"), "pictogramName");

            // Act
            var response = await client.PostAsync("/pictograms/", formData);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreatePictogram_ReturnsBadRequest_WhenPictogramNameIsMissing()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(_ => new BasicOrganizationSeeder());
            var client = factory.CreateClient();

            var testOrgId = 1;
                TestAuthHandler.TestClaims = new List<Claim>
                {
                    new Claim("OrgMember", testOrgId.ToString())
                };
            
            int organizationId;

            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var organization = await context.Organizations.FirstOrDefaultAsync();
                Assert.NotNull(organization);
                organizationId = organization.Id;
            }

            // Prepare multipart form data without pictogramName
            var formData = new MultipartFormDataContent();

            // Add image file
            var imageContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(imageContent, "image", "test.png");

            // Add organizationId
            formData.Add(new StringContent(organizationId.ToString()), "organizationId");

            // Act
            var response = await client.PostAsync("/pictograms/", formData);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Get Pictogram by ID Tests

        [Fact]
        public async Task GetPictogramById_ReturnsPictogram_WhenPictogramExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(_ => new BasicPictogramSeeder());
            var client = factory.CreateClient();

            int pictogramId;

            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var pictogram = await context.Pictograms.FirstOrDefaultAsync();
                Assert.NotNull(pictogram);
                pictogramId = pictogram.Id;
            }

            // Act
            var response = await client.GetAsync($"/pictograms/{pictogramId}");

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
            var factory = new GirafWebApplicationFactory(_ => new EmptyDb());
            var client = factory.CreateClient();
            int nonExistentPictogramId = 9999;

            // Act
            var response = await client.GetAsync($"/pictograms/{nonExistentPictogramId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region Get Pictograms by Organization ID Tests

        [Fact]
        public async Task GetPictogramsByOrganizationId_ReturnsPictograms_WhenPictogramsExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory(_ => new BasicPictogramSeeder());
            var client = factory.CreateClient();

            var testOrgId = 1;
                TestAuthHandler.TestClaims = new List<Claim>
                {
                    new Claim("OrgMember", testOrgId.ToString())
                };

            int organizationId;

            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var organization = await context.Organizations.FirstOrDefaultAsync();
            Assert.NotNull(organization);
            organizationId = organization.Id;

            var currentPage = 1;
            var pageSize = 10;

            // Act
            var response = await client.GetAsync($"/pictograms/organizationId:int?organizationId={organizationId}&currentPage={currentPage}&pageSize={pageSize}");

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
            var factory = new GirafWebApplicationFactory(_ => new BasicOrganizationSeeder());
            var client = factory.CreateClient();

            var testOrgId = 1;
                TestAuthHandler.TestClaims = new List<Claim>
                {
                    new Claim("OrgMember", testOrgId.ToString())
                };

            int organizationId;

            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var organization = await context.Organizations.FirstOrDefaultAsync();
                Assert.NotNull(organization);
                organizationId = organization.Id;
            }

            var currentPage = 1;
            var pageSize = 10;

            // Act
            var response = await client.GetAsync($"/pictograms/organizationId:int?organizationId={organizationId}&currentPage={currentPage}&pageSize={pageSize}");

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
            var factory = new GirafWebApplicationFactory(_ => new BasicPictogramSeeder());
            var client = factory.CreateClient();

            int pictogramId;

            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var pictogram = await context.Pictograms.FirstOrDefaultAsync();
                Assert.NotNull(pictogram);
                pictogramId = pictogram.Id;
            }

            // Act
            var response = await client.DeleteAsync($"/pictograms/{pictogramId}");

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
            var factory = new GirafWebApplicationFactory(_ => new EmptyDb());
            var client = factory.CreateClient();

            int nonExistentPictogramId = 9999;

            // Act
            var response = await client.DeleteAsync($"/pictograms/{nonExistentPictogramId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion
    }
}
