using Giraf.IntegrationTests.Utils;
using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Entities.Users.DTOs;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Giraf.IntegrationTests.Endpoints
{
    [Collection("IntegrationTests")]
    public class LoginEndpointTests
    {
        [Fact]
        public async Task Login_ReturnsOk_WithValidCredentials()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            seeder.SeedUsers(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            var client = factory.CreateClient();

            var loginDto = new LoginDTO
            {
                Username = seeder.Users["member"].UserName,
                Password = "Password123!"
            };

            // Act
            var response = await client.PostAsJsonAsync("/login", loginDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(content?.Token);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WithInvalidUsername()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            seeder.SeedUsers(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            var client = factory.CreateClient();

            var loginDto = new LoginDTO
            {
                Username = "invaliduser",
                Password = "Password123!"
            };

            // Act
            var response = await client.PostAsJsonAsync("/login", loginDto);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WithInvalidPassword()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            seeder.SeedUsers(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            var client = factory.CreateClient();

            var loginDto = new LoginDTO
            {
                Username = seeder.Users["member"].UserName,
                Password = "WrongPassword!"
            };

            // Act
            var response = await client.PostAsJsonAsync("/login", loginDto);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Helper class for deserializing the response
        private class LoginResponse
        {
            public required string Token { get; set; }
        }
    }
}
