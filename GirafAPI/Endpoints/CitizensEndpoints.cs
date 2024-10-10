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
            await dbContext.Citizens
                .Select(citizen => citizen.ToDTO())
                .AsNoTracking()
                .ToListAsync()
            )
            .WithName("GetAllCitizens")
            .WithTags("Citizens")
            .WithDescription("Retrieves a list of all citizens.")
            .Produces<List<CitizenDTO>>(StatusCodes.Status200OK);

        // GET /citizens/{id}
        group.MapGet("/{id:int}", async (int id, GirafDbContext dbContext) =>
            {
                Citizen? citizen = await dbContext.Citizens.FindAsync(id);
        
                return citizen is null ? Results.NotFound() : Results.Ok(citizen.ToDTO());
            })
            .WithName("GetCitizenById")
            .WithTags("Citizens")
            .WithDescription("Retrieves a citizen by their ID.")
            .Produces<CitizenDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // POST /citizens
        group.MapPost("/", async (CreateCitizenDTO newCitizen, GirafDbContext dbContext) =>
        {
            Citizen citizen = newCitizen.ToEntity();
    
            dbContext.Citizens.Add(citizen);
            await dbContext.SaveChangesAsync();
    
            return Results.CreatedAtRoute("GetCitizen", new { id = citizen.Id }, citizen.ToDTO());
        })
        .WithName("CreateCitizen")
        .WithTags("Citizens")
        .WithDescription("Creates a new citizen.")
        .Accepts<CreateCitizenDTO>("application/json")
        .Produces<CitizenDTO>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .WithParameterValidation();

        // PUT /citizens
        group.MapPut("/{id:int}", async (int id, UpdateCitizenDTO updatedCitizen, GirafDbContext dbContext) =>
        {
            var citizen = await dbContext.Citizens.FindAsync(id);

            if (citizen is null)
            {
                return Results.NotFound();
            }

            var activities = citizen.Activities;
    
            dbContext.Entry(citizen).CurrentValues.SetValues(updatedCitizen.ToEntity(id, activities));
            await dbContext.SaveChangesAsync();
    
            return Results.Ok();
        })
        .WithName("UpdateCitizen")
        .WithTags("Citizens")
        .WithDescription("Updates an existing citizen.")
        .Accepts<UpdateCitizenDTO>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);
        
        // DELETE /citizens/{id}
        group.MapDelete("/{id:int}", async (int id, GirafDbContext dbContext) =>
        {
            await dbContext.Citizens.Where(citizen => citizen.Id == id).ExecuteDeleteAsync();
    
            return Results.NoContent();
        })
        .WithName("DeleteCitizen")
        .WithTags("Citizens")
        .WithDescription("Deletes a citizen by their ID.")
        .Produces(StatusCodes.Status204NoContent);

        return group;
    }
}