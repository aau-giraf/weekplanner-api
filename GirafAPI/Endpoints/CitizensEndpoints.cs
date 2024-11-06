using GirafAPI.Data;
using GirafAPI.Entities.Resources;
using GirafAPI.Entities.Resources.DTOs;
using GirafAPI.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Endpoints;

// Endpoints for Trustees and Administrators to view, edit and delete Citizen data.
public static class CitizensEndpoints
{
    public static RouteGroupBuilder MapCitizensEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("citizens");

        // GET /citizens
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
            .Accepts<UpdateCitizenDTO>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);


        return group;
    }
}