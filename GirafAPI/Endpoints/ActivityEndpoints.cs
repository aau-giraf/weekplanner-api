using GirafAPI.Data;
using GirafAPI.Entities.Weekplans;
using GirafAPI.Entities.Weekplans.DTOs;
using GirafAPI.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Endpoints;

public static class ActivityEndpoints
{
    public static RouteGroupBuilder MapActivityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("weekplan");
        
        // GET all activities (mainly for debugging)
        group.MapGet("/", async (GirafDbContext dbContext) =>
            await dbContext.Activities
                .Select(a => a.ToDTO())
                .AsNoTracking()
                .ToListAsync()
        );
        
        // GET activities for one day for citizen
        group.MapGet("/{citizenId}", async (int citizenId, string date, GirafDbContext dbContext) =>
            await dbContext.Activities
                .Where(a => a.CitizenId == citizenId)
                .Where(a => a.Date == DateOnly.Parse(date))
                .OrderBy(a => a.StartTime)
                .Select(a => a.ToDTO())
                .AsNoTracking()
                .ToListAsync()
        );
        
        // GET single activity
        group.MapGet("/activity/{id}", async (int id, GirafDbContext dbContext) => 
        {
            Activity? activity = await dbContext.Activities.FindAsync(id);
            
            return activity is null ? Results.NotFound() : Results.Ok(activity.ToDTO());
        });
        
        // POST new activity
        group.MapPost("/{citizenId}", async (int citizenId, CreateActivityDTO newActivityDto, GirafDbContext dbContext) => 
        {
            Activity activity = newActivityDto.ToEntity(citizenId);
                
            dbContext.Activities.Add(activity);
            await dbContext.SaveChangesAsync();
            return Results.Created($"/activity/{activity.Id}", activity.ToDTO());
        });
        
        // POST copy activity
        group.MapPost("/activity/copy", async (int citizenId, List<int> ids, string dateStr, string newDateStr, GirafDbContext dbContext) => 
        {
            var date = DateOnly.Parse(dateStr);
            var newDate = DateOnly.Parse(newDateStr);

            var result = await dbContext.Activities
                .Where(a => a.CitizenId == citizenId)
                .Where(a => a.Date == date)
                .Where(a => ids.Contains(a.Id))
                .AsNoTracking()
                .ToListAsync();
            
            if(result is null)
            {
                return Results.NotFound();
            }

            foreach(Activity activity in result) 
            {
                dbContext.Activities.Add(new Activity
                {
                    CitizenId = citizenId,
                    Date = newDate,
                    Name = activity.Name,
                    Description = activity.Description,
                    StartTime = activity.StartTime,
                    EndTime = activity.EndTime
                });
            }
            
            await dbContext.SaveChangesAsync();

            return Results.Ok();
        });
        
        // PUT updated activity
        group.MapPut("/activity/{id}", async (int id, UpdateActivityDTO updatedActivity, GirafDbContext dbContext) =>
        {
            var activity = await dbContext.Activities.FindAsync(id);

            if (activity is null)
            {
                return Results.NotFound();
            }
            
            dbContext.Entry(activity).CurrentValues.SetValues(updatedActivity.ToEntity(id));
            await dbContext.SaveChangesAsync();
            
            return Results.Ok();
        });

        // DELETE activity
        group.MapDelete("/activity/{id}", async (int id, GirafDbContext dbContext) =>
        {
            await dbContext.Activities.Where(a => a.Id == id).ExecuteDeleteAsync();
            
            return Results.NoContent();
        });

        return group;
    }
}