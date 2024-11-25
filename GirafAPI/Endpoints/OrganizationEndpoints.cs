using System.Security.Claims;
using GirafAPI.Data;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Organizations.DTOs;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GirafAPI.Mapping;
using GirafAPI.Entities.Grades;

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

                    await dbContext.Entry(user)
                        .Collection(u => u.Organizations).LoadAsync();

                    var organizations = new List<OrganizationNameOnlyDTO>();
                    
                    foreach (var organization in user.Organizations)
                    {
                        organizations.Add(organization.ToNameOnlyDTO());
                    }

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
            .Produces<List<OrganizationNameOnlyDTO>>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("/{orgId}", async (int orgId, GirafDbContext dbContext) =>
            {
                try
                {
                    Organization? organization = await dbContext.Organizations.FindAsync(orgId);
                    if (organization is null)
                    {
                        return Results.NotFound();
                    }

                    await dbContext.Entry(organization)
                        .Collection(o => o.Users).LoadAsync();
                    await dbContext.Entry(organization)
                        .Collection(o => o.Citizens).LoadAsync();
                    await dbContext.Entry(organization)
                        .Collection(o => o.Grades).LoadAsync();

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
            .RequireAuthorization("OrganizationMember")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/",
                async (CreateOrganizationDTO newOrganization, GirafDbContext dbContext,
                    UserManager<GirafUser> userManager, HttpContext httpContext) =>
                {
                    try
                    {
                        var userClaims = httpContext.User.Claims;
                        foreach (var claim in userClaims)
                        {
                            Console.WriteLine($"{claim.Type}: {claim.Value}");
                        }
                        
                        var userId = userManager.GetUserId(httpContext.User);
                        
                        var user = userManager.FindByIdAsync(userId).GetAwaiter().GetResult();

                        if (user is null)
                        {
                            return Results.BadRequest("Invalid user id.");
                        }

                        Organization organization = newOrganization.ToEntity(user);
                        dbContext.Organizations.Add(organization);
                        await dbContext.SaveChangesAsync();
                        
                        var memberClaim = new Claim("OrgMember", organization.Id.ToString());
                        await userManager.AddClaimAsync(user, memberClaim);
                        
                        return Results.Created($"organizations/{organization.Id}", organization.ToDTO());
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                    }
                })
            .RequireAuthorization()
            .WithName("PostOrganization")
            .WithDescription("Creates a new organization.")
            .WithTags("Organizations")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapPut("/{orgId}/change-name", async (int orgId, string newName, GirafDbContext dbContext) =>
            {
                try
                {
                    Organization? organization = await dbContext.Organizations.FindAsync(orgId);
                    if (organization is null)
                    {
                        return Results.NotFound();
                    }

                    organization.Name = newName;
                    await dbContext.SaveChangesAsync();

                    return Results.Ok(organization.ToNameOnlyDTO());
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

        group.MapDelete("/{orgId}", async (int orgId, GirafDbContext dbContext) =>
            {
                try
                {
                    Organization? organization = await dbContext.Organizations.FindAsync(orgId);

                    if (organization is null)
                    {
                        return Results.NotFound();
                    }

                    await dbContext.Organizations.Where(o => o.Id == orgId).ExecuteDeleteAsync();
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

        group.MapPut("/{orgId}/remove-user/{userId}",
                async (int orgId, string userId, UserManager<GirafUser> userManager, GirafDbContext dbContext) =>
                {
                    try
                    {
                        var user = await userManager.FindByIdAsync(userId);
                        if (user is null)
                        {
                            return Results.BadRequest("Invalid user id.");
                        }

                        var organization = await dbContext.Organizations.FindAsync(orgId);
                        if (organization is null)
                        {
                            return Results.BadRequest("Invalid organization id.");
                        }

                        await dbContext.Entry(organization)
                            .Collection(o => o.Users).LoadAsync();
                        await dbContext.Entry(organization)
                            .Collection(o => o.Citizens).LoadAsync();

                        organization.Users.Remove(user);

                        await dbContext.SaveChangesAsync();
                        
                        var claims = await userManager.GetClaimsAsync(user);
                        var claimToRemove = claims.FirstOrDefault(c => c.Type == "OrgMember" && c.Value == organization.Id.ToString());
                        var result = await userManager.RemoveClaimAsync(user, claimToRemove);

                        return !result.Succeeded ? Results.BadRequest("Failed to remove organization claim.") : Results.Ok(organization.ToDTO());
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
        
        group.MapGet("/grades/{orgId}", async (int orgId, GirafDbContext dbContext) => {
            try
            {
                Grade? grade = await dbContext.Grades.FindAsync(orgId);
                if (grade is null)
                {
                    return Results.NotFound();
                }
                var organizationId = grade.OrganizationId;
                Organization? organization = await dbContext.Organizations.FindAsync(organizationId);
                if (organization is null)
                {
                    return Results.NotFound();
                }

                await dbContext.Entry(organization)
                    .Collection(o => o.Users).LoadAsync();
                await dbContext.Entry(organization)
                    .Collection(o => o.Citizens).LoadAsync();
                await dbContext.Entry(organization)
                    .Collection(o => o.Grades).LoadAsync();

                return Results.Ok(organization.ToDTO());         
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
            })
            .WithName("GetOrganizationByGradeId")
            .WithDescription("Gets organization by grade id.")
            .WithTags("Organizations")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/{orgId}/add-admin/{userId}",
            async (int orgId, string userId, UserManager<GirafUser> userManager, GirafDbContext dbContext) =>
            {
                try
                {
                    var user = await userManager.FindByIdAsync(userId);
                    if (user is null)
                    {
                        return Results.BadRequest("Invalid user id.");
                    }

                    var organization = await dbContext.Organizations.FindAsync(orgId);
                    if (organization is null)
                    {
                        return Results.BadRequest("Invalid organization id.");
                    }
                    
                    var adminClaim = new Claim("OrgAdmin", organization.Id.ToString());
                    var result = await userManager.AddClaimAsync(user, adminClaim);
                    if (!result.Succeeded)
                    {
                        return Results.BadRequest("Failed to add organization admin claim.");
                    }
                    
                    return Results.Ok();
                    
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                }

            })
            .WithName("AddOrganizationAdmin")
            .WithDescription("Adds an admin to an organization.")
            .WithTags("Organizations")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapPost("/{orgId}/remove-admin/{userId}",
            async (int orgId, string userId, UserManager<GirafUser> userManager, GirafDbContext dbContext) =>
            {
                try
                {
                    var user = await userManager.FindByIdAsync(userId);
                    if (user is null)
                    {
                        return Results.BadRequest("Invalid user id.");
                    }

                    var organization = await dbContext.Organizations.FindAsync(orgId);
                    if (organization is null)
                    {
                        return Results.BadRequest("Invalid organization id.");
                    }
                    
                    var claims = await userManager.GetClaimsAsync(user);
                    var claimToRemove = claims.FirstOrDefault(c => c.Type == "OrgAdmin" && c.Value == organization.Id.ToString());
                    var result = await userManager.RemoveClaimAsync(user, claimToRemove);
                    if (!result.Succeeded)
                    {
                        return Results.BadRequest("Failed to add organization admin claim.");
                    }
                    
                    return Results.Ok();
                    
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                }

            })
            .WithName("RemoveOrganizationAdmin")
            .WithDescription("Removes an admin from an organization.")
            .WithTags("Organizations")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        return group;
    }
}