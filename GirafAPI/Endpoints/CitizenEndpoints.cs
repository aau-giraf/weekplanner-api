using GirafAPI.Data;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Citizens.DTOs;
using GirafAPI.Entities.Activities;
using GirafAPI.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Endpoints;

// Endpoints for Trustees and Administrators to view, edit and delete Citizen data.
public static class CitizenEndpoints
{
    public static RouteGroupBuilder MapCitizensEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("citizens");

        // GET /citizens (mainly for debugging)
        group.MapGet("/", async (GirafDbContext dbContext) =>
            {
                try
                {
                    var citizens = await dbContext.Citizens
                        .Select(citizen => citizen.ToDTO())
                        .AsNoTracking()
                        .ToListAsync();

                    return Results.Ok(citizens);
                }
                catch (Exception ex)
                {
                    return Results.Problem("An unexpected error occurred while retrieving citizens." + ex.Message);
                }
            })
            .WithName("GetAllCitizens")
            .WithTags("Citizens")
            .WithDescription("Retrieves a list of all citizens.")
            .RequireAuthorization()
            .Produces<List<CitizenDTO>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        // GET /citizens/{id}
        group.MapGet("/{id:int}", async (int id, GirafDbContext dbContext) =>
            {
                try
                {
                    Citizen? citizen = await dbContext.Citizens.FindAsync(id);
                    return citizen is null ? Results.NotFound("Citizen not found.") : Results.Ok(citizen.ToDTO());
                }
                catch (Exception ex)
                {
                    return Results.Problem("An unexpected error occurred while retrieving the citizen." + ex.Message);
                }
            })
            .WithName("GetCitizenById")
            .WithTags("Citizens")
            .WithDescription("Retrieves a citizen by their ID.")
            .RequireAuthorization()
            .Produces<CitizenDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
        
        // PUT /citizens
        group.MapPut("/{id:int}", async (int id, UpdateCitizenDTO updatedCitizen, GirafDbContext dbContext) =>
            {
                try
                {
                    var citizen = await dbContext.Citizens
                        .Include(c => c.Activities)
                        .Include(c => c.Organization)
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (citizen is null)
                    {
                        return Results.NotFound("Citizen not found.");
                    }

                    var activities = citizen.Activities;
                    var organization = citizen.Organization;
                    if (citizen.Organization == null)
                    {
                        return Results.BadRequest("The citizen's organization is not set.");
                    }
                    if (citizen.Activities == null)
                    {
                        citizen.Activities = new List<Activity>();
                    }
                    dbContext.Entry(citizen).CurrentValues.SetValues(updatedCitizen.ToEntity(id, activities, organization));
                    await dbContext.SaveChangesAsync();

                    return Results.Ok();
                }
                catch (DbUpdateException ex)
                {
                    // Database update error
                    return Results.BadRequest("There was an error updating the citizen data. " + ex.Message);
                }
                catch (Exception ex)
                {
                    // Unexpected
                    return Results.Problem("An unexpected error occurred while updating the citizen." + ex.Message);
                }
            })
            .WithName("UpdateCitizen")
            .WithTags("Citizens")
            .WithDescription("Updates an existing citizen.")
            .RequireAuthorization()
            .Accepts<UpdateCitizenDTO>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapPost("/{orgId}/add-citizen",
                async (int orgId, CreateCitizenDTO newCitizen, GirafDbContext dbContext) =>
                {
                    try
                    {
                        var organization = await dbContext.Organizations.FindAsync(orgId);
                        if (organization is null)
                        {
                            return Results.NotFound();
                        }

                        var citizen = newCitizen.ToEntity(organization);
                        dbContext.Citizens.Add(citizen);

                        await dbContext.Entry(organization)
                            .Collection(o => o.Citizens).LoadAsync();

                        organization.Citizens.Add(citizen);

                        await dbContext.SaveChangesAsync();

                        return Results.Ok(citizen.Id);
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                    }
                })
            .WithName("AddCitizen")
            .WithDescription("Add citizen to organization.")
            .WithTags("Organizations")
            .RequireAuthorization("OrganizationAdmin")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapDelete("/{orgId}/remove-citizen/{citizenId}",
                async (int orgId, int citizenId, GirafDbContext dbContext) =>
                {
                    try
                    {
                        var citizen = await dbContext.Citizens
                            .Include(c => c.Organization)
                            .FirstOrDefaultAsync(c => c.Id == citizenId);
                        
                        if (citizen is null)
                        {
                            return Results.NotFound();
                        }
                        
                        if (citizen.Organization.Id != orgId)
                        {
                            return Results.BadRequest("Citizen does not belong to the specified organization.");
                        }

                        dbContext.Citizens.Remove(citizen);
                        await dbContext.SaveChangesAsync();

                        return Results.NoContent();
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                    }
                })
            .WithName("RemoveCitizen")
            .WithDescription("Remove citizen from organization.")
            .WithTags("Organizations")
            .RequireAuthorization("OrganizationAdmin")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        return group;
    }
}