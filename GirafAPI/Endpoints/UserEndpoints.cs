using GirafAPI.Entities.DTOs;
using GirafAPI.Entities.Users;
using GirafAPI.Entities.Users.DTOs;
using GirafAPI.Mapping;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Endpoints;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("users");

        // POST /users
        group.MapPost("/", async (CreateUserDTO newUser, UserManager<GirafUser> userManager) =>
        {
            GirafUser user = newUser.ToEntity();
            var result = await userManager.CreateAsync(user, newUser.Password);
            return result.Succeeded ? Results.Created($"/users/{user.Id}", user.ToDTO()) : Results.BadRequest();
        })
        .WithName("CreateUser")
        .WithTags("Users")
        .WithDescription("Creates a new user with the specified details.")
        .Accepts<CreateUserDTO>("application/json")
        .Produces<GirafUser>(StatusCodes.Status201Created)
        .Produces<IEnumerable<IdentityError>>(StatusCodes.Status400BadRequest);

        // PUT /users/{id}
        group.MapPut("/{userId}", async (string userId, UpdateUserDTO updatedUser, UserManager<GirafUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return Results.NotFound();
            }

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
          
            var result = await userManager.UpdateAsync(user);
            return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
          })
          .WithName("UpdateUserDetails")
          .WithTags("Users")
          .WithDescription("Updates an existing user's details by their ID.")
          .RequireAuthorization("OwnData")
          .Accepts<UpdateUserDTO>("application/json")
          .Produces(StatusCodes.Status200OK)
          .Produces<IEnumerable<IdentityError>>(StatusCodes.Status400BadRequest)
          .Produces(StatusCodes.Status404NotFound);
        
        // Get /users/{id}
        group.MapGet("/{userId}", async (string userId, UserManager<GirafUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(user.ToDTO());
        })
        .WithName("GetUser")
        .WithTags("Users")
        .WithDescription("Returns a user by id")
        .RequireAuthorization("OwnData")
        .Produces<UserDTO>()
        .Produces<NotFound>(StatusCodes.Status404NotFound);
        
        group.MapPut("/{userId}/change-password", async (string userId, UpdateUserPasswordDTO updatePasswordDTO, UserManager<GirafUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            
            if(user == null) {
                return Results.BadRequest("Invalid user id.");
            }

            var result = await userManager.ChangePasswordAsync(
                user, 
                updatePasswordDTO.oldPassword, 
                updatePasswordDTO.newPassword);
            return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
        })
        .WithName("ChangeUserPassword")
        .WithTags("Users")
        .WithDescription("Allows a user to change their password.")
        .RequireAuthorization("OwnData")
        .Accepts<UpdateUserPasswordDTO>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<IEnumerable<IdentityError>>(StatusCodes.Status400BadRequest);
        
        group.MapPut("/{userId}/change-username", async (string userId, UpdateUsernameDTO updateUsernameDTO, UserManager<GirafUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);

            if(user == null) {
                return Results.BadRequest("Invalid user id.");
            }

            var result = await userManager.SetUserNameAsync(user, updateUsernameDTO.Username);
            return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
        })
        .WithName("ChangeUsername")
        .WithTags("Users")
        .WithDescription("Allows a user to change their username.")
        .RequireAuthorization("OwnData")
        .Accepts<UpdateUsernameDTO>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<IEnumerable<IdentityError>>(StatusCodes.Status400BadRequest);

        //[FromBody] is needed by ASP NETs .MapDelete method
        group.MapDelete("/{userId}", async ([FromBody] DeleteUserDTO deleteUserDTO, UserManager<GirafUser> userManager) =>
        {
            try {
                var user = await userManager.FindByIdAsync(deleteUserDTO.Id);

                if(user == null) {
                    return Results.BadRequest("Invalid user id.");
                }
                
                var passwordValid = await userManager.CheckPasswordAsync(user, deleteUserDTO.Password);

                if(!passwordValid) {
                    return Results.BadRequest("Invalid password");
                }
                
                await userManager.DeleteAsync(user);
                return Results.NoContent();
            }
            catch (Exception) 
            {
                //unexpected error
                return Results.Problem("An error occurred while trying to delete user.", statusCode: StatusCodes.Status500InternalServerError);
            }
            
        })
        .WithName("DeleteUser")
        .WithTags("Users")
        .WithDescription("Deletes a user by their ID.")
        .RequireAuthorization("OwnData")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<IEnumerable<IdentityError>>(StatusCodes.Status400BadRequest);
        
        
        group.MapPost("/{userId}/setProfilePicture", async (string userId, [FromForm] IFormFile image, UserManager<GirafUser> userManager) =>
            {
                if (image.Length < 0)
                {
                    return Results.BadRequest("Image file is required");
                }

                var user = await userManager.FindByIdAsync(userId);

                if (user is null)
                {
                    return Results.NotFound("User not found");
                }
    
                var folderPath = Path.Combine("wwwroot", "images", "users");
    
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = user.Id;
                var filePath = Path.Combine(folderPath, fileName + ".jpeg");
                await using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);
                return Results.Ok();
            })
            .DisableAntiforgery()
            .WithName("SetProfilePicture")
            .WithDescription("Set the user's profile picture")
            .WithTags("Users")
            .RequireAuthorization("OwnData")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);



    return group;
    }
}