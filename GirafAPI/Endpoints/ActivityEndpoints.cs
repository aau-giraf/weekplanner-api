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
            .RequireAuthorization()
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
                    return Results.Problem("An error occurred while retrieving activities.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("GetActivitiesForCitizenOnDate")
            .WithDescription("Gets activities for a specific citizen on a given date.")
            .WithTags("Activities")
            .RequireAuthorization()
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
                    return Results.Problem("An error occurred while retrieving activities.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("GetActivitiesForGradeOnDate")
            .WithDescription("Gets activities for a specific grade on a given date.")
            .WithTags("Activities")
            .RequireAuthorization()
            .Produces<List<ActivityDTO>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);


        // GET single activity by ID
        group.MapGet("/activity/{id:int}", async (int id, GirafDbContext dbContext) =>
            {
                try
                {
                    Activity? activity = await dbContext.Activities.Include(a => a.Pictogram)
                        .FirstOrDefaultAsync(a => a.Id == id);

                    //activity is not found
                    return activity is null ? Results.NotFound("Activity not found.") : Results.Ok(activity.ToDTO());
                }
                catch (Exception)
                {
                    //unexpected error
                    return Results.Problem("An error occurred while retrieving the activity.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("GetActivityById")
            .WithDescription("Gets a specific activity by ID.")
            .WithTags("Activities")
            .RequireAuthorization()
            .Produces<ActivityDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);


        // POST new activity
        group.MapPost("/to-citizen/{citizenId:int}",
                async (int citizenId, CreateActivityDTO newActivityDto, GirafDbContext dbContext) =>
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
                        return Results.Problem("An error occurred while creating the activity.",
                            statusCode: StatusCodes.Status500InternalServerError);
                    }
                })
            .WithName("CreateActivityForCitizen")
            .WithDescription("Creates a new activity for a citizen.")
            .WithTags("Activities")
            .RequireAuthorization()
            .Accepts<CreateActivityDTO>("application/json")
            .Produces<ActivityDTO>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        // POST new activity
        group.MapPost("/to-grade/{gradeId:int}",
                async (int gradeId, CreateActivityDTO newActivityDto, GirafDbContext dbContext) =>
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
                        return Results.Problem("An error occurred while creating the activity.",
                            statusCode: StatusCodes.Status500InternalServerError);
                    }
                })
            .WithName("CreateActivityForGrade")
            .WithDescription("Creates a new activity for a grade.")
            .WithTags("Activities")
            .RequireAuthorization()
            .Accepts<CreateActivityDTO>("application/json")
            .Produces<ActivityDTO>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);


        group.MapPost("/activity/copy-citizen/{citizenId:int}",
                async (int citizenId, string dateStr, string newDateStr, List<int> toCopyIds,
                    GirafDbContext dbContext) =>
                {
                    try
                    {
                        var citizen = await dbContext.Citizens.FindAsync(citizenId);
                        if (citizen is null)
                        {
                            return Results.NotFound();
                        }

                        var sourceDate = DateOnly.Parse(dateStr);
                        var targetDate = DateOnly.Parse(newDateStr);

                        var activities = await dbContext.Entry(citizen)
                            .Collection(c => c.Activities)
                            .Query()
                            .Include(a => a.Pictogram)
                            .Where(a => a.Date == sourceDate || toCopyIds.Contains(a.Id))
                            .ToListAsync();

                        if (activities.Count is 0)
                        {
                            return Results.NotFound("No activities found for the given date.");
                        }

                        foreach (var a in activities)
                        {
                            var newActivity = new Activity
                            {
                                Date = targetDate,
                                StartTime = a.StartTime,
                                EndTime = a.EndTime,
                                IsCompleted = false,
                                Pictogram = a.Pictogram
                            };

                            await dbContext.Entry(citizen)
                                .Collection(c => c.Activities).LoadAsync();
                            citizen.Activities.Add(newActivity);
                        }

                        await dbContext.SaveChangesAsync();
                        return Results.Ok("Activities successfully copied.");
                    }
                    catch (FormatException)
                    {
                        return Results.BadRequest(
                            "Invalid date format. Please provide the date in 'YYYY-MM-DD' format.");
                    }
                    catch (Exception)
                    {
                        return Results.Problem("An error occurred while copying the activities.",
                            statusCode: StatusCodes.Status500InternalServerError);
                    }
                })
            .WithName("CopyActivityForCitizen")
            .WithDescription("Copies activities between days for a citizen")
            .WithTags("Activities")
            .RequireAuthorization()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK)
            .RequireAuthorization();

        group.MapPost("/activity/copy-grade/{gradeId:int}",
                async (int gradeId, string dateStr, string newDateStr, List<int> toCopyIds, GirafDbContext dbContext) =>
                {
                    try
                    {
                        var grade = await dbContext.Grades.FindAsync(gradeId);
                        if (grade is null)
                        {
                            return Results.NotFound();
                        }

                        var sourceDate = DateOnly.Parse(dateStr);
                        var targetDate = DateOnly.Parse(newDateStr);

                        var activities = await dbContext.Entry(grade)
                            .Collection(c => c.Activities)
                            .Query()
                            .Include(a => a.Pictogram)
                            .Where(a => a.Date == sourceDate || toCopyIds.Contains(a.Id))
                            .ToListAsync();

                        if (activities.Count is 0)
                        {
                            return Results.NotFound("No activities found for the given date.");
                        }

                        foreach (var a in activities)
                        {
                            var newActivity = new Activity
                            {
                                Date = targetDate,
                                StartTime = a.StartTime,
                                EndTime = a.EndTime,
                                IsCompleted = false,
                                Pictogram = a.Pictogram
                            };

                            await dbContext.Entry(grade)
                                .Collection(c => c.Activities).LoadAsync();
                            grade.Activities.Add(newActivity);
                        }

                        await dbContext.SaveChangesAsync();
                        return Results.Ok("Activities successfully copied.");
                    }
                    catch (FormatException)
                    {
                        return Results.BadRequest(
                            "Invalid date format. Please provide the date in 'YYYY-MM-DD' format.");
                    }
                    catch (Exception)
                    {
                        return Results.Problem("An error occurred while copying the activities.",
                            statusCode: StatusCodes.Status500InternalServerError);
                    }
                })
            .WithName("CopyActivityForGrade")
            .WithDescription("Copies activities between days for a grade")
            .WithTags("Activities")
            .RequireAuthorization()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK)
            .RequireAuthorization();

        // PUT updated activity
        group.MapPut("/activity/{id:int}",
                async (int id, UpdateActivityDTO updatedActivity, GirafDbContext dbContext) =>
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
                        return Results.Problem("An error occurred while updating the activity.",
                            statusCode: StatusCodes.Status500InternalServerError);
                    }
                })
            .WithName("UpdateActivity")
            .WithDescription("Updates an existing activity using ID.")
            .WithTags("Activities")
            .RequireAuthorization()
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
                    return Results.Problem("An error occurred while updating the activity status.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("CompleteActivity")
            .WithDescription("Completes an existing activity using ID.")
            .WithTags("Activities")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);


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
                    return Results.Problem("An error occurred while deleting the activity.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("DeleteActivity")
            .WithDescription("Deletes an activity by ID.")
            .WithTags("Activities")
            .RequireAuthorization()
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
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);


        return group;
    }
}