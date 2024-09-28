using GirafAPI.Data;
using GirafAPI.Entities.Resources;
using GirafAPI.Entities.Resources.DTOs;
using GirafAPI.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Endpoints;

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
            );

        // GET /citizens/{id}
        group.MapGet("/{id:int}", async (int id, GirafDbContext dbContext) =>
            {
                Citizen? citizen = await dbContext.Citizens.FindAsync(id);
        
                return citizen is null ? Results.NotFound() : Results.Ok(citizen.ToDTO());
            })
            .WithName("GetCitizen");

        // POST /citizens
        group.MapPost("/", async (CreateCitizenDTO newCitizen, GirafDbContext dbContext) =>
        {
            Citizen citizen = newCitizen.ToEntity();
    
            dbContext.Citizens.Add(citizen);
            await dbContext.SaveChangesAsync();
    
            return Results.CreatedAtRoute("GetCitizen", new { id = citizen.Id }, citizen.ToDTO());
        }).WithParameterValidation();

        // PUT /citizens
        group.MapPut("/{id:int}", async (int id, UpdateCitizenDTO updatedCitizen, GirafDbContext dbContext) =>
        {
            var citizen = await dbContext.Citizens.FindAsync(id);

            if (citizen is null)
            {
                return Results.NotFound();
            }
    
            dbContext.Entry(citizen).CurrentValues.SetValues(updatedCitizen.ToEntity(id));
            await dbContext.SaveChangesAsync();
    
            return Results.Ok();
        });

        // DELETE /citizens/{id}
        group.MapDelete("/{id:int}", async (int id, GirafDbContext dbContext) =>
        {
            await dbContext.Citizens.Where(citizen => citizen.Id == id).ExecuteDeleteAsync();
    
            return Results.NoContent();
        });

        return group;
    }
}