using GirafAPI.Data;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Organizations.DTOs;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GirafAPI.Mapping;

namespace GirafAPI.Endpoints;

public static class OrganizationEndpoints
{
    public static RouteGroupBuilder MapOrganizationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("organizations");

        group.MapGet("/user/{id}", async (string id, GirafDbContext dbContext, UserManager<GirafUser> userManager) =>
        {
            try
            {
                var user = await userManager.FindByIdAsync(id);

                if (user is null)
                {
                    return Results.BadRequest("Invalid user id.");
                }

                if (user.Organizations is null)
                {
                    return Results.NotFound();
                }

                var organizationIds = new List<int>();
                foreach (var organization in user.Organizations)
                {
                    organizationIds.Add(organization.Id);
                }

                var organizations = await dbContext.Organizations
                    .Where(organization => organizationIds.Contains(organization.Id))
                    .Select(org => org.ToNameOnlyDTO())
                    .AsNoTracking()
                    .ToListAsync();

                return Results.Ok(organizations);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }

        })
        .WithName("GetOrganizations")    
        .WithDescription("Gets organizations for user.")
        .WithTags("Organizations")
        .Produces<List<OrganizationThumbnailDTO>>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id}", async (int id, GirafDbContext dbContext) =>
        {
            try
            {
                Organization? organization = await dbContext.Organizations.FindAsync(id);
                if (organization is null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(organization.ToDTO());
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetOrganizationById")
        .WithDescription("Gets organization by id.")
        .WithTags("Organizations")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/", async (string id, CreateOrganizationDTO newOrganization, GirafDbContext dbContext, UserManager<GirafUser> userManager) =>
            {
                try
                {
                    var user = await userManager.FindByIdAsync(id);
                    
                    if (user is null)
                    {
                        return Results.BadRequest("Invalid user id.");
                    }
                    
                    Organization organization = newOrganization.ToEntity(user);
                    dbContext.Organizations.Add(organization);
                    await dbContext.SaveChangesAsync();
                    return Results.Created($"organizations/{organization.Id}", organization.ToDTO());
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("PostOrganization")    
            .WithDescription("Creates a new organization.")
            .WithTags("Organizations")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapPut("/{id}/change-name", async (int id, string newName, GirafDbContext dbContext) =>
        {
            try
            {
                Organization? organization = await dbContext.Organizations.FindAsync(id);
                if (organization is null)
                {
                    return Results.NotFound();
                }

                organization.Name = newName;
                await dbContext.SaveChangesAsync();

                return Results.Ok(organization.ToDTO());
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("ChangeOrganizationName")
        .WithDescription("Changes the name of the organization.")
        .WithTags("Organizations")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapDelete("/{id}", async (int id, GirafDbContext dbContext) =>
        {
            try
            {
                Organization? organization = await dbContext.Organizations.FindAsync(id);

                if (organization is null)
                {
                    return Results.NotFound();
                }

                await dbContext.Organizations.Where(o => o.Id == id).ExecuteDeleteAsync();
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("DeleteOrganization")
        .WithDescription("Deletes the organization.")
        .WithTags("Organizations")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapPut("/{id}/remove-user/{userId}",
            async (int id, string userId, UserManager<GirafUser> userManager, GirafDbContext dbContext) =>
            {
                try
                {
                    var user = await userManager.FindByIdAsync(userId);
                    if (user is null)
                    {
                        return Results.BadRequest("Invalid user id.");
                    }

                    var organization = await dbContext.Organizations.FindAsync(id);
                    if (organization is null)
                    {
                        return Results.BadRequest("Invalid organization id.");
                    }

                    organization.Users.Remove(user);
                    if (organization.Admins.Contains(user))
                    {
                        organization.Admins.Remove(user);
                    }
                    await dbContext.SaveChangesAsync();

                    return Results.Ok(organization.ToDTO());
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("RemoveUser")
            .WithDescription("Removes user from organization.")
            .WithTags("Organizations")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapPut("/{id}/permissions/{userId}",
            async (int id, string userId, bool makeAdmin,UserManager<GirafUser> userManager, GirafDbContext dbContext) =>
            {
                try
                {
                    var user = await userManager.FindByIdAsync(userId);
                    if (user is null)
                    {
                        return Results.BadRequest("Invalid user id.");
                    }

                    var organization = await dbContext.Organizations.FindAsync(id);
                    if (organization is null)
                    {
                        return Results.BadRequest("Invalid organization id.");
                    }

                    if (makeAdmin)
                    {
                        if (organization.Admins.Contains(user))
                        {
                            return Results.BadRequest("User is already an admin.");
                        }

                        organization.Admins.Add(user);
                    }
                    else
                    {
                        if (!organization.Admins.Contains(user))
                        {
                            return Results.BadRequest("User is not an admin.");
                        }

                        organization.Admins.Remove(user);
                    }

                    return Results.Ok(organization.ToDTO());
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("ManagePermissions")
            .WithDescription("Manage organization permissions.")
            .WithTags("Organizations")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
        
        return group;
    }
}