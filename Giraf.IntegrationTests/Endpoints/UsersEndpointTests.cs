using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Giraf.IntegrationTests.Utils;
using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Data;
using GirafAPI.Entities.DTOs;
using GirafAPI.Entities.Users.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Giraf.IntegrationTests.Endpoints;

public class UsersEndpointTests
{
    // 1. Test POST /users - Creating a new user successfully
    [Fact]
    public async Task CreateUser_ReturnsCreated_WhenUserIsValid()
    {
        // Arrange
        var seeder = new EmptyDb();
        var factory = new GirafWebApplicationFactory(seeder);
        var client = factory.CreateClient();

        var newUserDto = new CreateUserDTO
        {
            FirstName = "newuser",
            LastName = "UserEndpountTest",
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
        var seeder = new EmptyDb();
        var factory = new GirafWebApplicationFactory(seeder);
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

    // 3. Test PUT /users/{id} when updating an existing user successfully
    [Fact]
    public async Task UpdateUser_ReturnsOk_WhenUserExists()
    {
        // Arrange
        var seeder = new BasicUserSeeder();
        var factory = new GirafWebApplicationFactory(seeder);
        var client = factory.CreateClient();

        // Retrieve the actual user ID from the database after seeding
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
        var existingUser = await dbContext.Users.FirstOrDefaultAsync();
        Assert.NotNull(existingUser);

        var updateUserDto = new UpdateUserDTO("UpdatedFirstName", "UpdatedLastName");

        // Act
        var response = await client.PutAsJsonAsync($"/users/{existingUser.Id}", updateUserDto);

        // Assert
        response.EnsureSuccessStatusCode();

        // Verify that the user was updated
        var updatedUserResponse = await client.GetAsync($"/users/{existingUser.Id}");
        updatedUserResponse.EnsureSuccessStatusCode();
        var updatedUser = await updatedUserResponse.Content.ReadFromJsonAsync<UserDTO>();
        Assert.NotNull(updatedUser);
        Assert.Equal("UpdatedFirstName", updatedUser.FirstName);
        Assert.Equal("UpdatedLastName", updatedUser.LastName);
    }

    // 4. Test PUT /users/{id} when the user does not exist
    [Fact]
    public async Task UpdateUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var seeder = new EmptyDb();
        var factory = new GirafWebApplicationFactory(seeder);
        var client = factory.CreateClient();

        var updateUserDto = new UpdateUserDTO("NonExistentFirstName", "NonExistentLastName");
        var nonExistentUserId = "nonexistent_user_id";

        // Act
        var response = await client.PutAsJsonAsync($"/users/{nonExistentUserId}", updateUserDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // 5. Test GET /users/ when there are users in the DB
    [Fact]
    public async Task GetUsers_ReturnsListOfUsers()
    {
        // Arrange
        var seeder = new MultipleUsersSeeder();
        var factory = new GirafWebApplicationFactory(seeder);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/users");

        // Assert
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<UserDTO>>();
        Assert.NotNull(users);
        Assert.NotEmpty(users);
    }

    // 6. Test GET /users when there are no users
    [Fact]
    public async Task GetUsers_ReturnsNotFound_WhenNoUsersExist()
    {
        // Arrange
        var seeder = new EmptyDb();
        var factory = new GirafWebApplicationFactory(seeder);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/users");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // 7. Test GET /users/{id} when the user exists
    [Fact]
    public async Task GetUserById_ReturnsUser_WhenUserExists()
    {
        // Arrange
        var seeder = new BasicUserSeeder();
        var factory = new GirafWebApplicationFactory(seeder);
        var client = factory.CreateClient();

        // Retrieve the actual user ID
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
        var existingUser = await dbContext.Users.FirstOrDefaultAsync();
        Assert.NotNull(existingUser);

        // Act
        var response = await client.GetAsync($"/users/{existingUser.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<UserDTO>();
        Assert.NotNull(user);
        Assert.Equal(existingUser.Id, user.Id);
    }

    // 8. Test GET /users/{id} when the user does not exist
    [Fact]
    public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var seeder = new EmptyDb();
        var factory = new GirafWebApplicationFactory(seeder);
        var client = factory.CreateClient();

        var nonExistentUserId = "nonexistent_user_id";

        // Act
        var response = await client.GetAsync($"/users/{nonExistentUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // 9. Test PUT /users/{id}/change-password when the user and old password are valid
    [Fact]
    public async Task ChangeUserPassword_ReturnsOk_WhenUserAndOldPasswordAreValid()
    {
        // Arrange
        var seeder = new BasicUserWithPasswordSeeder();
        var factory = new GirafWebApplicationFactory(seeder);
        var client = factory.CreateClient();

        // Retrieve the actual user ID and old password from the seeder
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "basicuser");
        Assert.NotNull(user);

        var updatePasswordDto = new UpdateUserPasswordDTO
        (
            oldPassword : BasicUserWithPasswordSeeder.SeededUserPassword,
            newPassword : "NewP@ssw0rd!"
        );

        // Act
        var response = await client.PutAsJsonAsync($"/users/{user.Id}/change-password", updatePasswordDto);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    // 10. Test PUT /users/{id}/change-password when the user does not exist
    [Fact]
    public async Task ChangeUserPassword_ReturnsBadRequest_WhenUserDoesNotExist()
    {
        // Arrange
        var seeder = new BasicUserWithPasswordSeeder();
        var factory = new GirafWebApplicationFactory(seeder);
        var client = factory.CreateClient();

        var nonExistentUserId = "nonexistent_user_id";
        var updatePasswordDto = new UpdateUserPasswordDTO
        (
            oldPassword : "P@ssw0rd!",
            newPassword : "NewP@ssw0rd!"
        );

        // Act
        var response = await client.PutAsJsonAsync($"/users/{nonExistentUserId}/change-password", updatePasswordDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // 11. Test PUT /users/{id}/change-username when the user and new username are valid
    [Fact]
    public async Task ChangeUsername_ReturnsOk_WhenUserAndUsernameAreValid()
    {
        // Arrange
        var seeder = new BasicUserSeeder();
        var factory = new GirafWebApplicationFactory(seeder);
        var client = factory.CreateClient();

        // Retrieve the actual user ID
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "basicuser");
        Assert.NotNull(user);

        var updateUsernameDto = new UpdateUsernameDTO
        (
            Username : "updateduser"
        );

        // Act
        var response = await client.PutAsJsonAsync($"/users/{user.Id}/change-username", updateUsernameDto);

        // Assert
        response.EnsureSuccessStatusCode();

        // Verify username change by retrieving updated user data
        var updatedUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal("updateduser", updatedUser.UserName);
    }

    // 12. Test PUT /users/{id}/change-username when the user does not exist
    [Fact]
    public async Task ChangeUsername_ReturnsBadRequest_WhenUserDoesNotExist()
    {
        // Arrange
        var seeder = new BasicUserSeeder();
        var factory = new GirafWebApplicationFactory(seeder);
        var client = factory.CreateClient();

        var nonExistentUserId = "nonexistent_user_id";
        var updateUsernameDto = new UpdateUsernameDTO
        (
            Username : "anotherusername"
        );

        // Act
        var response = await client.PutAsJsonAsync($"/users/{nonExistentUserId}/change-username", updateUsernameDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // 13. Test DELETE /users/{id} when the user exists
    [Fact]
    public async Task DeleteUser_ReturnsNoContent_WhenUserExists()
    {
        // Arrange
        var seeder = new BasicUserSeeder();
        var factory = new GirafWebApplicationFactory(seeder);
        var client = factory.CreateClient();

        // Retrieve the actual user ID
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
        var user = await dbContext.Users.FirstOrDefaultAsync();
        Assert.NotNull(user);

        // Act
        var response = await client.DeleteAsync($"/users/{user.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify that the user was removed
        var deletedUser = await dbContext.Users.FindAsync(user.Id);
        Assert.Null(deletedUser);
    }

    // 14. Test DELETE /users/{id} when the user does not exist
    [Fact]
    public async Task DeleteUser_ReturnsBadRequest_WhenUserDoesNotExist()
    {
        // Arrange
        var seeder = new EmptyDb();
        var factory = new GirafWebApplicationFactory(seeder);
        var client = factory.CreateClient();

        var nonExistentUserId = "nonexistent_user_id";

        // Act
        var response = await client.DeleteAsync($"/users/{nonExistentUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}