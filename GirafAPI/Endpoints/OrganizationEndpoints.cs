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
                    .Select(org => org.ToThumbnailDTO())
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
        
        return group;
    }
}