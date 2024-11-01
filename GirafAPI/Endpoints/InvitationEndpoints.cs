using Azure;
using GirafAPI.Data;
using GirafAPI.Entities.Invitations;
using GirafAPI.Entities.Invitations.DTOs;
using GirafAPI.Entities.Users;
using GirafAPI.Mapping;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Endpoints;

public static class InvitationEndpoints
{
    public static RouteGroupBuilder MapInvitationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("invitations");
        
        group.MapGet("/{id}", async (int id, GirafDbContext dbContext) =>
            {
                try
                {
                    Invitation? invitation = await dbContext.Invitations.FindAsync(id);
                    
                    return invitation is null ? Results.NotFound() : Results.Ok(invitation);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("GetInvitationById")
            .WithDescription("Get invitation by id.")
            .WithTags("Invitation")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapGet("/org/{id}", async (int id, GirafDbContext dbContext) =>
            {
                try
                {
                    var invitations = await dbContext.Invitations
                        .Where(i => i.OrganizationId == id)
                        .ToListAsync();
                    
                    return Results.Ok(invitations);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("GetInvitationByOrg")
            .WithDescription("Get all invitations for an organization.")
            .WithTags("Invitation")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapPost("/", async (CreateInvitationDTO newInvitation, GirafDbContext dbContext) =>
        {
            try
            {
                Invitation invitation = newInvitation.ToEntity();

                dbContext.Invitations.Add(invitation);
                await dbContext.SaveChangesAsync();
                return Results.Created($"/invitations/{invitation.Id}", invitation);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("CreateInvitation")
        .WithDescription("Creates a new invitation.")
        .WithTags("Invitation")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapPut("/respond/{id}", async (int id, bool response, GirafDbContext dbContext, UserManager<GirafUser> userManager) =>
            {
                try
                {
                    Invitation? invitation = await dbContext.Invitations.FindAsync(id);

                    if (invitation is null)
                    {
                        return Results.NotFound();
                    }

                    if (response)
                    {
                        var organization = await dbContext.Organizations.FindAsync(invitation.OrganizationId);

                        if (organization is null)
                        {
                            return Results.NotFound();
                        }
                        
                        dbContext.Entry(organization)
                            .Collection(o => o.Users).Load();

                        var user = await userManager.FindByIdAsync(invitation.ReceiverId);
                        if (user is null)
                        {
                            return Results.NotFound();
                        }
                        
                        organization.Users.Add(user);
                        await dbContext.SaveChangesAsync();
                    }
                    
                    await dbContext.Invitations.Where(i => i.Id == id).ExecuteDeleteAsync();
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("RespondToInvitation")
            .WithDescription("Accept or reject invitation.")
            .WithTags("Invitation")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapDelete("/{id}", async (int id, GirafDbContext dbContext) =>
            {
                try
                {
                    Invitation? invitation = await dbContext.Invitations.FindAsync(id);

                    if (invitation is null)
                    {
                        return Results.NotFound();
                    }
                    
                    await dbContext.Invitations.Where(i => i.Id == id).ExecuteDeleteAsync();
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("DeleteInvitation")
            .WithDescription("Delete invitation.")
            .WithTags("Invitation")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        return group;
    }
}