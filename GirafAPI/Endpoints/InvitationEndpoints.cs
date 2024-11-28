using System.Security.Claims;
using GirafAPI.Data;
using GirafAPI.Entities.Invitations;
using GirafAPI.Entities.Invitations.DTOs;
using GirafAPI.Entities.Organizations;
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

                    if (invitation is null)
                    {
                        return Results.NotFound("Invitation not found.");
                    }
                    
                    GirafUser? sender = await dbContext.Users.FindAsync(invitation.SenderId);

                    if (sender is null)
                    {
                        return Results.NotFound("Invitation sender not found.");
                    }
                    
                    Organization? organization = await dbContext.Organizations.FindAsync(invitation.OrganizationId);

                    if (organization is null)
                    {
                        return Results.NotFound("Organization not found.");
                    }
                    
                    var invitationDTO = invitation.ToDTO(organization.Name, $"{sender.FirstName} {sender.LastName}");
                    
                    return Results.Ok(invitationDTO);
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

        group.MapGet("/user/{userId}", async (string userId, GirafDbContext dbContext) =>
        {
            try
            {
                var invitations = await dbContext.Invitations
                    .Where(i => i.ReceiverId == userId)
                    .ToListAsync();

                if (invitations.Count == 0)
                {
                    return Results.Ok(Array.Empty<InvitationDTO>());
                }

                var invitationDtos = new List<InvitationDTO>();
                foreach (var invitation in invitations)
                {
                    GirafUser? sender = await dbContext.Users.FindAsync(invitation.SenderId);
                    if (sender is null)
                    {
                        continue;
                    }
                    Organization? organization = await dbContext.Organizations.FindAsync(invitation.OrganizationId);
                    if (organization is null)
                    {
                        continue;
                    }
                    var invitationDto = invitation.ToDTO(organization.Name, $"{sender.FirstName} {sender.LastName}");
                    invitationDtos.Add(invitationDto);
                }

                return Results.Ok(invitationDtos);
            }
            catch (Exception e)
            {
                return Results.Problem(e.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetInvitationsByUserId")
        .WithDescription("Get all invitations for user.")
        .WithTags("Invitation")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);
        
        
        group.MapGet("/org/{id}", async (int id, GirafDbContext dbContext) =>
            {
                try
                {
                    var invitations = await dbContext.Invitations
                        .Where(i => i.OrganizationId == id)
                        .ToListAsync();

                    if (invitations.Count == 0)
                    {
                        return Results.NotFound("No invitations found.");
                    }
                    
                    var invitationDtos = new List<InvitationDTO>();
                    foreach (var invitation in invitations)
                    {
                        GirafUser? sender = await dbContext.Users.FindAsync(invitation.SenderId);
                        if (sender is null)
                        {
                            return Results.NotFound("Invitation sender not found.");
                        }
                        Organization? organization = await dbContext.Organizations.FindAsync(invitation.OrganizationId);
                        if (organization is null)
                        {
                            return Results.NotFound("Organization not found.");
                        }
                        var invitationDto = invitation.ToDTO(organization.Name, $"{sender.FirstName} {sender.LastName}");
                        invitationDtos.Add(invitationDto);
                    }

                    return Results.Ok(invitationDtos);
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
        
        group.MapPost("/", async (CreateInvitationDTO newInvitation, GirafDbContext dbContext, UserManager<GirafUser> userManager) =>
        {
            try
            {
                var receiver = await userManager.FindByEmailAsync(newInvitation.ReceiverEmail);
                if (receiver == null)
                {
                    return Results.BadRequest("Receiver email not found.");
                }

                var invitation = newInvitation.ToEntity(receiver.Id);
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
        
        group.MapPut("/respond/{id}", async (int id, InvitationResponseDTO responseDto, GirafDbContext dbContext, UserManager<GirafUser> userManager) =>
            {
                try
                {
                    Invitation? invitation = await dbContext.Invitations.FindAsync(id);

                    if (invitation is null)
                    {
                        return Results.NotFound();
                    }

                    if (responseDto.Response)
                    {
                        var organization = await dbContext.Organizations.FindAsync(invitation.OrganizationId);

                        if (organization is null)
                        {
                            return Results.NotFound();
                        }
                        
                        await dbContext.Entry(organization)
                            .Collection(o => o.Users).LoadAsync();

                        var user = await userManager.FindByIdAsync(invitation.ReceiverId);
                        if (user is null)
                        {
                            return Results.NotFound();
                        }
                        
                        organization.Users.Add(user);
                        await dbContext.SaveChangesAsync();
                        
                        var claim = new Claim("OrgMember", organization.Id.ToString());
                        var result = await userManager.AddClaimAsync(user, claim);
                        if (!result.Succeeded)
                        {
                            return Results.BadRequest("Failed to add organisation claim.");
                        }
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