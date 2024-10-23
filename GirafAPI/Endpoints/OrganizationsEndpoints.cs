using GirafAPI.Data;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Organizations.DTOs;
using GirafAPI.Entities.Users;
using GirafAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace GirafAPI.Endpoints
{
    public static class OrganizationEndpoints
    {
        public static RouteGroupBuilder MapOrganizationEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("organizations");
            // POST /organizations
            group.MapPost("/", async (CreateOrganizationDTO newOrgDTO, UserManager<GirafUser> userManager, GirafDbContext dbContext, ClaimsPrincipal userClaims) =>
            {
                var userId = userClaims.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Results.Unauthorized();
                }
                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Results.Unauthorized();
                }
                var organization = newOrgDTO.ToEntity();
                try
                {
                    dbContext.Organizations.Add(organization);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return Results.Problem("An error occurred while creating the organization.", statusCode: StatusCodes.Status500InternalServerError);
                }
                // Assign the creating user to the organization and give admin role
                user.OrganizationId = organization.Id;
                var roleResult = await userManager.AddToRoleAsync(user, "OrganizationAdmin");
                if (!roleResult.Succeeded)
                {
                    // Handle role assignment failure
                    return Results.Problem("Failed to assign OrganizationAdmin role to the user.", statusCode: StatusCodes.Status500InternalServerError);
                }
                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    // Handle user update failure
                    return Results.Problem("Failed to update the user with the organization ID.", statusCode: StatusCodes.Status500InternalServerError);
                }
                return Results.Created($"/organizations/{organization.Id}", organization.ToDTO());
            })
            .WithName("CreateOrganization")
            .WithTags("Organizations")
            .WithDescription("Creates a new organization and assigns the current user as its administrator.")
            .Accepts<CreateOrganizationDTO>("application/json")
            .Produces<OrganizationDTO>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);
            // GET /organizations/{id}
            group.MapGet("/{id:int}", async (int id, UserManager<GirafUser> userManager, GirafDbContext dbContext, ClaimsPrincipal userClaims) =>
            {
                var userId = userClaims.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Results.Unauthorized();
                }
                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Results.Unauthorized();
                }
                Organization organization;
                try
                {
                    organization = await dbContext.Organizations.FindAsync(id);
                }
                catch (Exception)
                {
                    return Results.Problem("An error occurred while retrieving the organization.", statusCode: StatusCodes.Status500InternalServerError);
                }
                if (organization == null)
                {
                    return Results.NotFound();
                }
                if (user.OrganizationId != organization.Id)
                {
                    return Results.Forbid();
                }
                return Results.Ok(organization.ToDTO());
            })
            .WithName("GetOrganizationById")
            .WithTags("Organizations")
            .WithDescription("Retrieves an organization by its ID.")
            .Produces<OrganizationDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

            // PUT /organizations/{id}
            group.MapPut("/{id:int}", async (int id, UpdateOrganizationDTO dto, UserManager<GirafUser> userManager, GirafDbContext dbContext, ClaimsPrincipal userClaims) =>
            {
                var userId = userClaims.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Results.Unauthorized();
                }
                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Results.Unauthorized();
                }
                var organization = await dbContext.Organizations.FindAsync(id);
                if (organization == null)
                {
                    return Results.NotFound();
                }
                if (user.OrganizationId != organization.Id)
                {
                    return Results.Forbid();
                }
                organization.UpdateFromDTO(dto, dbContext);
                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return Results.Problem("An error occurred while updating the organization.", statusCode: StatusCodes.Status500InternalServerError);
                }
                return Results.Ok();
            })
            .WithName("UpdateOrganization")
            .WithTags("Organizations")
            .WithDescription("Updates an existing organization.")
            .Accepts<UpdateOrganizationDTO>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

            // DELETE /organizations/{id}
            group.MapDelete("/{id:int}", async (int id, UserManager<GirafUser> userManager, GirafDbContext dbContext, ClaimsPrincipal userClaims) =>
            {
                var userId = userClaims.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Results.Unauthorized();
                }
                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Results.Unauthorized();
                }
                var organization = await dbContext.Organizations.FindAsync(id);
                if (organization == null)
                {
                    return Results.NotFound();
                }
                if (user.OrganizationId != organization.Id)
                {
                    return Results.Forbid();
                }
                dbContext.Organizations.Remove(organization);
                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return Results.Problem("An error occurred while deleting the organization.", statusCode: StatusCodes.Status500InternalServerError);
                }
                return Results.NoContent();
            })
            .WithName("DeleteOrganization")
            .WithTags("Organizations")
            .WithDescription("Deletes an organization.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

            // POST /organizations/{organizationId}/users
            group.MapPost("/{organizationId:int}/users", async (int organizationId, AddUserToOrganizationDTO dto, UserManager<GirafUser> userManager, GirafDbContext dbContext, ClaimsPrincipal userClaims) =>
            {
                var adminUserId = userClaims.FindFirstValue(ClaimTypes.NameIdentifier);
                if (adminUserId == null)
                {
                    return Results.Unauthorized();
                }
                var adminUser = await userManager.FindByIdAsync(adminUserId);
                if (adminUser == null)
                {
                    return Results.Unauthorized();
                }
                if (adminUser.OrganizationId != organizationId)
                {
                    return Results.Forbid();
                }
                var user = await userManager.FindByIdAsync(dto.UserId);
                if (user == null)
                {
                    return Results.NotFound("User not found.");
                }
                user.OrganizationId = organizationId;
                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    // Handle update failure
                    return Results.Problem("Failed to add user to the organization.", statusCode: StatusCodes.Status500InternalServerError);
                }
                return Results.Ok();
            })
            .WithName("AddUserToOrganization")
            .WithTags("Organizations")
            .WithDescription("Adds a user to an organization.")
            .Accepts<AddUserToOrganizationDTO>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

            // DELETE /organizations/{organizationId}/users/{userId}
            group.MapDelete("/{organizationId:int}/users/{userId}", async (int organizationId, string userId, UserManager<GirafUser> userManager, GirafDbContext dbContext, ClaimsPrincipal userClaims) =>
            {
                var adminUserId = userClaims.FindFirstValue(ClaimTypes.NameIdentifier);
                if (adminUserId == null)
                {
                    return Results.Unauthorized();
                }
                var adminUser = await userManager.FindByIdAsync(adminUserId);
                if (adminUser == null)
                {
                    return Results.Unauthorized();
                }
                if (adminUser.OrganizationId != organizationId)
                {
                    return Results.Forbid();
                }
                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Results.NotFound("User not found.");
                }
                if (user.OrganizationId != organizationId)
                {
                    return Results.Forbid();
                }
                var deleteResult = await userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    return Results.Problem("Failed to delete the user.", statusCode: StatusCodes.Status500InternalServerError);
                }
                return Results.Ok();
            })
            .WithName("DeleteUserFromOrganization")
            .WithTags("Organizations")
            .WithDescription("Deletes a user from an organization.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
            return group;
        }
    }
}

