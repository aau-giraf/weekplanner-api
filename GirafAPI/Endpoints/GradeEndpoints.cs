using GirafAPI.Data;
using GirafAPI.Entities.Grades;
using GirafAPI.Entities.Grades.DTOs;
using GirafAPI.Mapping;

namespace GirafAPI.Endpoints;

public static class GradeEndpoints
{
    public static RouteGroupBuilder MapGradeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("grades");
        
        group.MapGet("/{id}", async (int id, GirafDbContext dbContext) =>
        {
            try
            {
                var grade = await dbContext.Grades.FindAsync(id);
                return grade is null ? Results.NotFound() : Results.Ok(grade.ToDTO());
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetGradeById")
        .WithTags("Grade")
        .WithDescription("Gets a grade by id.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapGet("/org/{orgId}", async (int orgId, GirafDbContext dbContext) =>
        {
            try
            {
                var org = await dbContext.Organizations.FindAsync(orgId);
            }
        })
        
        group.MapPost("/", async (int orgId, CreateGradeDTO newGrade, GirafDbContext dbContext) =>
        {
            try
            {
                var organization = await dbContext.Organizations.FindAsync(orgId);
                if (organization == null)
                {
                    return Results.NotFound("Organization not found.");
                }

                var grade = newGrade.ToEntity();
                dbContext.Grades.Add(grade);
                await dbContext.SaveChangesAsync();
                return Results.Created($"grades/{grade.Id}", grade.ToDTO());
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("CreateGrade")
        .WithTags("Grade")
        .WithDescription("Creates a new grade.")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);    
        
        return group;
    }
}