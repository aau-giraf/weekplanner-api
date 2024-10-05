using GirafAPI.Data;
using GirafAPI.Entities.Resources;
using GirafAPI.Entities.Weekplans;
using GirafAPI.Entities.Weekplans.DTOs;
using GirafAPI.Mapping;
using Microsoft.VisualBasic;

namespace GirafAPI.Endpoints;

public static class DayplanEndpoints
{
    public static RouteGroupBuilder MapDayplanEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("dayplan");

        group.MapGet("/{citizenId}", async (int citizenId, 
                                                  int year, 
                                                  int month, 
                                                  int day, 
                                                  GirafDbContext dbContext) =>
        {
            Citizen? citizen = await dbContext.Citizens.FindAsync(citizenId);

            if (citizen is null)
            {
                return Results.NotFound();
            }
            
            DateOnly date = new(year, month, day);
            
            Dayplan? dayplan = await dbContext.Dayplans.FindAsync(citizenId, date);
            
            return dayplan is null ? Results.NotFound() : Results.Ok(dayplan);
        }).WithName("GetDayplan");
        
        group.MapPost("/{citizenId}", async (CreateDayplanDTO newDayplan, GirafDbContext dbContext) =>
        {
            Citizen? citizen = await dbContext.Citizens.FindAsync(newDayplan.CitizenId);

            if (citizen is null)
            {
                return Results.NotFound();
            }
            
            Dayplan? oldDayplan = await dbContext.Dayplans.FindAsync(newDayplan.CitizenId, DateOnly.Parse(newDayplan.Date));

            if (oldDayplan != null)
            {
                return Results.Conflict();
            }
            
            dbContext.Dayplans.Add(newDayplan.ToEntity());
            await dbContext.SaveChangesAsync();
            
            return Results.CreatedAtRoute("GetDayplan", new {citizenId = citizen.Id}, newDayplan);
        });

        return group;
    }
}