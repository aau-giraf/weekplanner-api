using System.Net;
using System.Net.Http.Json;
using Giraf.IntegrationTests.Utils;
using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Data;
using GirafAPI.Entities.DTOs;
using GirafAPI.Entities.Users;
using GirafAPI.Entities.Users.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Giraf.IntegrationTests.Endpoints
{
    [Collection("IntegrationTests")]
    public class UsersEndpointTests
    {
        #region Create User Tests

        // 1. Test POST /users - Creating a new user successfully
        [Fact]
        public async Task CreateUser_ReturnsCreated_WhenUserIsValid()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var client = factory.CreateClient();

            var newUserDto = new CreateUserDTO
            {
                FirstName = "newuser",
                LastName = "UserEndpointTest",
                Email = "newuser@example.com",
                Password = "P@ssw0rd!"
            };

            // Act
            var response = await client.PostAsJsonAsync("/users", newUserDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var createdUser = await response.Content.ReadFromJsonAsync<UserDTO>();
            Assert.NotNull(createdUser);
            Assert.Equal(newUserDto.Email, createdUser.Email);
            Assert.NotEmpty(createdUser.Id);
        }

        // 2. Test POST /users when the user details are invalid (e.g., missing password)
        [Fact]
        public async Task CreateUser_ReturnsBadRequest_WhenUserIsInvalid()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var client = factory.CreateClient();

            var newUserDto = new CreateUserDTO
            {
                FirstName = "invaliduser",
                LastName = "ForTestingPurposes",
                Email = "invaliduser@example.com",
                Password = ""  // invalid password
            };

            // Act
            var response = await client.PostAsJsonAsync("/users", newUserDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Update User Tests

        // 3. Test PUT /users/{id} when updating an existing user successfully
        [Fact]
        public async Task UpdateUser_ReturnsOk_WhenUserExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            seeder.SeedUsers(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            var userId = seeder.Users["member"].Id;

            var updateUserDto = new UpdateUserDTO("UpdatedFirstName", "UpdatedLastName");

            // Act
            var response = await client.PutAsJsonAsync($"/users/{userId}", updateUserDto);

            // Assert
            response.EnsureSuccessStatusCode();

            // Verify that the user was updated using a new DbContext
            using (var verificationScope = factory.Services.CreateScope())
            {
                var verificationDbContext = verificationScope.ServiceProvider.GetRequiredService<GirafDbContext>();
                var updatedUser = await verificationDbContext.Users.FindAsync(userId);
                Assert.NotNull(updatedUser);
                Assert.Equal("UpdatedFirstName", updatedUser.FirstName);
                Assert.Equal("UpdatedLastName", updatedUser.LastName);
            }
        }

        // 4. Test PUT /users/{id} when the user does not exist
        [Fact]
        public async Task UpdateUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            seeder.SeedUsers(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            var updateUserDto = new UpdateUserDTO("NonExistentFirstName", "NonExistentLastName");
            var nonExistentUserId = "nonexistent_user_id";

            // Act
            var response = await client.PutAsJsonAsync($"/users/{nonExistentUserId}", updateUserDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region Get Users Tests

        // 5. Test GET /users/{id} when the user exists
        [Fact]
        public async Task GetUserById_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            seeder.SeedUsers(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            var userId = seeder.Users["member"].Id;

            // Act
            var response = await client.GetAsync($"/users/{userId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var user = await response.Content.ReadFromJsonAsync<UserDTO>();
            Assert.NotNull(user);
            Assert.Equal(userId, user.Id);
        }

        // 6. Test GET /users/{id} when the user does not exist
        [Fact]
        public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            seeder.SeedUsers(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            var nonExistentUserId = "nonexistent_user_id";

            // Act
            var response = await client.GetAsync($"/users/{nonExistentUserId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region Change Password Tests

        // 7. Test PUT /users/{id}/change-password when the user and old password are valid
        [Fact]
        public async Task ChangeUserPassword_ReturnsOk_WhenUserAndOldPasswordAreValid()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            seeder.SeedUsers(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);
            
            var userId = seeder.Users["member"].Id;

            var updatePasswordDto = new UpdateUserPasswordDTO
            (
                oldPassword: "Password123!",
                newPassword: "NewP@ssw0rd!"
            );

            // Act
            var response = await client.PutAsJsonAsync($"/users/{userId}/change-password", updatePasswordDto);

            // Assert
            response.EnsureSuccessStatusCode();
        }


        // 8. Test PUT /users/{id}/change-password when the user does not exist
        [Fact]
        public async Task ChangeUserPassword_ReturnsBadRequest_WhenUserDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            seeder.SeedUsers(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            var nonExistentUserId = "nonexistent_user_id";
            var updatePasswordDto = new UpdateUserPasswordDTO
            (
                oldPassword: "P@ssw0rd!",
                newPassword: "NewP@ssw0rd!"
            );

            // Act
            var response = await client.PutAsJsonAsync($"/users/{nonExistentUserId}/change-password", updatePasswordDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Change Username Tests

        // 9. Test PUT /users/{id}/change-username when the user and new username are valid
        [Fact]
        public async Task ChangeUsername_ReturnsOk_WhenUserAndUsernameAreValid()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            seeder.SeedUsers(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            var userId = seeder.Users["member"].Id;
            var updateUsernameDto = new UpdateUsernameDTO("updatedUser");

            // Act
            var response = await client.PutAsJsonAsync($"/users/{userId}/change-username", updateUsernameDto);

            // Assert
            response.EnsureSuccessStatusCode();

            // Reload the user entity from the database
            var verificationScope = factory.Services.CreateScope();
            var userManager = verificationScope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>();
            var user = await userManager.FindByIdAsync(userId);

            Assert.NotNull(user);
            Assert.Equal("updatedUser", user.UserName);
        }

        // 10. Test PUT /users/{id}/change-username when the user does not exist
        [Fact]
        public async Task ChangeUsername_ReturnsBadRequest_WhenUserDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            seeder.SeedUsers(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            var nonExistentUserId = "nonexistent_user_id";
            var updateUsernameDto = new UpdateUsernameDTO("anotherusername");

            // Act
            var response = await client.PutAsJsonAsync($"/users/{nonExistentUserId}/change-username", updateUsernameDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Delete User Tests

        // 11. Test DELETE /users/{id} when the user exists
        [Fact]
        public async Task DeleteUser_ReturnsNoContent_WhenUserExists()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            seeder.SeedUsers(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            var userId = seeder.Users["member"].Id;

            var deleteUserDto = new DeleteUserDTO
            {
                Id = userId,
                Password = "Password123!"
            };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/users/{userId}")
            {
                Content = JsonContent.Create(deleteUserDto)
            };
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify that the user was removed using a new DbContext
            using var verificationScope = factory.Services.CreateScope();
            var verificationDbContext = verificationScope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var deletedUser = await verificationDbContext.Users.FindAsync(userId);
            Assert.Null(deletedUser);
        }

        // 12. Test DELETE /users/{id} when the user does not exist
        [Fact]
        public async Task DeleteUser_ReturnsBadRequest_WhenUserDoesNotExist()
        {
            // Arrange
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            seeder.SeedUsers(scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>());
            var client = factory.CreateClient();

            client.AttachClaimsToken(scope, seeder.Users["member"]);

            var nonExistentUserId = "nonexistent_user_id";

            // Act
            var response = await client.DeleteAsync($"/users/{nonExistentUserId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion
    }
}
