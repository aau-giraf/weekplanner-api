using GirafAPI.Data;
using GirafAPI.Entities.Activities;
using GirafAPI.Entities.Activities.DTOs;
using GirafAPI.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GirafAPI.Endpoints;

public static class ActivityEndpoints
{
    public static RouteGroupBuilder MapActivityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("weekplan");
        
        // GET all activities (mainly for debugging)
        group.MapGet("/", async (GirafDbContext dbContext) =>
        {
            try
            {
                var activities = await dbContext.Activities
                    .Include(a => a.Pictogram)
                    .Select(a => a.ToDTO())
                    .AsNoTracking()
                    .ToListAsync();

                return Results.Ok(activities);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetAllActivities")
        .WithDescription("Gets all activities.")
        .WithTags("Activities")
        .Produces<List<ActivityDTO>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

            
        // GET activities for one day for a citizen
        group.MapGet("/{citizenId:int}", async (int citizenId, string date, GirafDbContext dbContext) =>
        {
            try
            {
                var citizen = await dbContext.Citizens.FindAsync(citizenId);

                if (citizen is null)
                {
                    return Results.NotFound();
                }
                
                await dbContext.Entry(citizen)
                    .Collection(c => c.Activities)
                    .Query()
                    .Include(a => a.Pictogram)
                    .LoadAsync();
                
                var activities = new List<ActivityDTO>();

                foreach (var activity in citizen.Activities)
                {
                    if (activity.Date == DateOnly.Parse(date))
                    {
                        activities.Add(activity.ToDTO());
                    }
                }

                return Results.Ok(activities);
            }
            catch (Exception)
            {
                //unexpected error
                return Results.Problem("An error occurred while retrieving activities.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetActivitiesForCitizenOnDate")
        .WithDescription("Gets activities for a specific citizen on a given date.")
        .WithTags("Activities")
        .Produces<List<ActivityDTO>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);
        
        // GET activities for one day for a grade
        group.MapGet("/grade/{gradeId:int}", async (int gradeId, string date, GirafDbContext dbContext) =>
            {
                try
                {
                    var grade = await dbContext.Grades.FindAsync(gradeId);

                    if (grade is null)
                    {
                        return Results.NotFound();
                    }
                
                    await dbContext.Entry(grade)
                        .Collection(c => c.Activities)
                        .Query()
                        .Include(a => a.Pictogram)
                        .LoadAsync();

                    if (grade.Activities.IsNullOrEmpty())
                    {
                        return Results.NotFound();
                    }
                
                    var activities = new List<ActivityDTO>();

                    foreach (var activity in grade.Activities)
                    {
                        if (activity.Date == DateOnly.Parse(date))
                        {
                            activities.Add(activity.ToDTO());
                        }
                    }

                    return activities.IsNullOrEmpty() ? Results.NotFound() : Results.Ok(activities);
                }
                catch (Exception)
                {
                    //unexpected error
                    return Results.Problem("An error occurred while retrieving activities.", statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("GetActivitiesForGradeOnDate")
            .WithDescription("Gets activities for a specific grade on a given date.")
            .WithTags("Activities")
            .Produces<List<ActivityDTO>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

            
        // GET single activity by ID
        group.MapGet("/activity/{id:int}", async (int id, GirafDbContext dbContext) =>
        {
            try
            {
                Activity? activity = await dbContext.Activities.Include(a => a.Pictogram).FirstOrDefaultAsync(a => a.Id == id);
                
                //activity is not found
                return activity is null ? Results.NotFound("Activity not found.") : Results.Ok(activity.ToDTO());
            }
            catch (Exception)
            {
                //unexpected error
                return Results.Problem("An error occurred while retrieving the activity.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetActivityById")
        .WithDescription("Gets a specific activity by ID.")
        .WithTags("Activities")
        .Produces<ActivityDTO>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);

        
        // POST new activity
        group.MapPost("/to-citizen/{citizenId:int}", async (int citizenId, CreateActivityDTO newActivityDto, GirafDbContext dbContext) =>
        {
            try
            {
                Activity? activity;
                if (newActivityDto.PictogramId is not null)
                {
                    var pictogram = await dbContext.Pictograms.FindAsync(newActivityDto.PictogramId);
                    
                    activity = newActivityDto.ToEntity(pictogram);
                        
                }
                else
                {
                    activity = newActivityDto.ToEntity();
                }
                
                var citizen = await dbContext.Citizens.FindAsync(citizenId);
                
                if (citizen is null)
                {
                    return Results.NotFound("Citizen not found.");
                }
                
                await dbContext.Entry(citizen)
                    .Collection(c => c.Activities).LoadAsync();
                citizen.Activities.Add(activity);
                
                await dbContext.SaveChangesAsync();
                return Results.Created($"/activity/{activity.Id}", activity.ToDTO());
            }
            catch (Exception)
            {
                //unexpected errors
                return Results.Problem("An error occurred while creating the activity.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("CreateActivityForCitizen")
        .WithDescription("Creates a new activity for a citizen.")
        .WithTags("Activities")
        .Accepts<CreateActivityDTO>("application/json")
        .Produces<ActivityDTO>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);
        
        // POST new activity
        group.MapPost("/to-grade/{gradeId:int}", async (int gradeId, CreateActivityDTO newActivityDto, GirafDbContext dbContext) =>
            {
                try
                {
                    Activity? activity;
                    if (newActivityDto.PictogramId is not null)
                    {
                        var pictogram = await dbContext.Pictograms.FindAsync(newActivityDto.PictogramId);
                        
                        activity = newActivityDto.ToEntity(pictogram);
                        
                    }
                    else
                    {
                        activity = newActivityDto.ToEntity();
                    }
                
                    var grade = await dbContext.Grades.FindAsync(gradeId);
                
                    if (grade is null)
                    {
                        return Results.NotFound("Citizen not found.");
                    }
                
                    await dbContext.Entry(grade)
                        .Collection(c => c.Activities).LoadAsync();
                    grade.Activities.Add(activity);
                
                    await dbContext.SaveChangesAsync();
                    return Results.Created($"/activity/{activity.Id}", activity.ToDTO());
                }
                catch (Exception)
                {
                    //unexpected errors
                    return Results.Problem("An error occurred while creating the activity.", statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("CreateActivityForGrade")
            .WithDescription("Creates a new activity for a grade.")
            .WithTags("Activities")
            .Accepts<CreateActivityDTO>("application/json")
            .Produces<ActivityDTO>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);


        
       // POST copy activity
        group.MapPost("/activity/copy", async (int citizenId, string dateStr, string newDateStr, GirafDbContext dbContext) =>
        {
            try
            {
                var date = DateOnly.Parse(dateStr);
                var newDate = DateOnly.Parse(newDateStr);
                
                var citizen = await dbContext.Citizens.FindAsync(citizenId);

                if (citizen is null)
                {
                    return Results.NotFound();
                }
                
                await dbContext.Entry(citizen)
                    .Collection(c => c.Activities).LoadAsync();

                foreach (Activity activity in citizen.Activities)
                {
                    if (activity.Date == date)
                    {
                        citizen.Activities.Add(new Activity
                        {
                            Date = newDate,
                            StartTime = activity.StartTime,
                            EndTime = activity.EndTime,
                            IsCompleted = activity.IsCompleted
                        });
                    }
                }

                await dbContext.SaveChangesAsync();

                return Results.Ok();
            }
            catch (FormatException)
            {
                //date parsing errors
                return Results.BadRequest("Invalid date format. Please provide the date in 'YYYY-MM-DD' format.");
            }
            catch (Exception)
            {
                //unexpected errors
                return Results.Problem("An error occurred while copying the activities.", statusCode: StatusCodes.Status500InternalServerError);
            }
        });

        
        // PUT updated activity
        group.MapPut("/activity/{id:int}", async (int id, UpdateActivityDTO updatedActivity, GirafDbContext dbContext) =>
            {
                try
                {
                    var activity = await dbContext.Activities
                        .Include(a => a.Pictogram) 
                        .FirstOrDefaultAsync(a => a.Id == id);

                    if (activity is null)
                    {
                        return Results.NotFound("Activity not found.");
                    }
                    
                    var pictogram = await dbContext.Pictograms.FindAsync(updatedActivity.PictogramId);
                    if (pictogram is null)
                    {
                        return Results.BadRequest($"Pictogram with ID {updatedActivity.PictogramId} not found.");
                    }
                    
                    activity.Pictogram = pictogram; 
                    dbContext.Entry(activity).CurrentValues.SetValues(updatedActivity.ToEntity(id));
                    await dbContext.SaveChangesAsync();

                    return Results.Ok();
                }
                catch (DbUpdateException)
                {
                    // Database update issue
                    return Results.BadRequest("Failed to update activity. Ensure the provided data is correct.");
                }
                catch (Exception)
                {
                    // Server error for any unexpected errors
                    return Results.Problem("An error occurred while updating the activity.", statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("UpdateActivity")
            .WithDescription("Updates an existing activity using ID.")
            .WithTags("Activities")
            .Accepts<UpdateActivityDTO>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);


        // PUT IsComplete activity
        group.MapPut("/activity/{id:int}/iscomplete", async (int id, bool IsComplete, GirafDbContext dbContext) =>
        {
            try
            {
                var activity = await dbContext.Activities.FindAsync(id);

                if (activity is null)
                {
                    return Results.NotFound("Activity not found.");
                }

                activity.IsCompleted = IsComplete;
                await dbContext.SaveChangesAsync();

                return Results.Ok();
            }
            catch (DbUpdateException)
            {
                // database update issues
                return Results.BadRequest("Failed to update activity status.");
            }
            catch (Exception)
            {
                // unexpected errors
                return Results.Problem("An error occurred while updating the activity status.", statusCode: StatusCodes.Status500InternalServerError);
            }
        });


        // DELETE activity
        group.MapDelete("/activity/{id:int}", async (int id, GirafDbContext dbContext) =>
        {
            try
            {
                var rowsAffected = await dbContext.Activities.Where(a => a.Id == id).ExecuteDeleteAsync();

                if (rowsAffected == 0)
                {
                    return Results.NotFound("Activity not found.");
                }

                return Results.NoContent();
            }
            catch (DbUpdateException)
            {
                // database update or delete issues
                return Results.BadRequest("Failed to delete activity. Ensure the ID is correct.");
            }
            catch (Exception)
            {
                // unexpected errors
                return Results.Problem("An error occurred while deleting the activity.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("DeleteActivity")
        .WithDescription("Deletes an activity by ID.")
        .WithTags("Activities")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/activity/assign-pictogram/{activityId:int}/{pictogramId:int}",
            async (int activityId, int pictogramId, GirafDbContext dbContext) =>
            {
                try
                {
                    var activity = await dbContext.Activities.FindAsync(activityId);

                    if (activity is null)
                    {
                        return Results.NotFound("Activity not found.");
                    }
                    
                    var pictogram = await dbContext.Pictograms.FindAsync(pictogramId);

                    if (pictogram is null)
                    {
                        return Results.NotFound("Pictogram not found.");
                    }
                    
                    activity.Pictogram = pictogram;
                    await dbContext.SaveChangesAsync();

                    return Results.Ok(activity.ToDTO());

                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                }

            })
            .WithName("AssignPictogram")
            .WithDescription("Assigns a pictogram by ID.")
            .WithTags("Activities")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);


        return group;
    }
}