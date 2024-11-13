using GirafAPI.Data;
using GirafAPI.Entities.Grades.DTOs;
using GirafAPI.Mapping;
using Microsoft.EntityFrameworkCore;

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

                if (grade is null)
                {
                    return Results.NotFound("Grade not found.");
                }
                
                await dbContext.Entry(grade)
                    .Collection(g => g.Citizens).LoadAsync();
                
                return Results.Ok(grade.ToDTO());
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
                if (org is null)
                {
                    return Results.NotFound("Organization not found.");
                }

                var grades = dbContext.Grades
                    .Where(g => g.OrganizationId == orgId)
                    .Select(g => g.ToDTO())
                    .AsNoTracking()
                    .ToList();

                return Results.Ok(grades);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetGradesInOrganization")
        .WithTags("Grade")
        .WithDescription("Get all grades within organization.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapPost("/", async (int orgId, CreateGradeDTO newGrade, GirafDbContext dbContext) =>
        {
            try
            {
                var organization = await dbContext.Organizations.FindAsync(orgId);
                if (organization == null)
                {
                    return Results.NotFound("Organization not found.");
                }
                
                await dbContext.Entry(organization)
                    .Collection(o => o.Grades).LoadAsync();

                var grade = newGrade.ToEntity(orgId);
                organization.Grades.Add(grade);
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
        
        group.MapPut("/{id}/change-name", async (int id, string newName, GirafDbContext dbContext) =>
        {
            try
            {
                var grade = await dbContext.Grades.FindAsync(id);

                if (grade is null)
                {
                    return Results.NotFound("Grade not found.");
                }

                grade.Name = newName;
                await dbContext.SaveChangesAsync();
                return Results.Ok(grade.ToDTO());
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("ChangeGradeName")
        .WithTags("Grade")
        .WithDescription("Change name of grade.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapPut("/{gradeId}/add-citizen/{citizenId}", async (int gradeId, int citizenId, GirafDbContext dbContext) =>
            {
                try
                {
                    var grade = await dbContext.Grades.FindAsync(gradeId);

                    if (grade is null)
                    {
                        return Results.NotFound("Grade not found.");
                    }

                    await dbContext.Entry(grade)
                        .Collection(g => g.Citizens).LoadAsync();
                    
                    var citizen = await dbContext.Citizens.FindAsync(citizenId);

                    if (citizen is null)
                    {
                        return Results.NotFound("Citizen not found.");
                    }

                    grade.Citizens.Add(citizen);
                    await dbContext.SaveChangesAsync();
                    return Results.Ok(grade.ToDTO());
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("AddCitizenToGrade")
            .WithTags("Grade")
            .WithDescription("Add a citizen to a grade.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapPut("/{gradeId}/remove-citizen/{citizenId}", async (int gradeId, int citizenId, GirafDbContext dbContext) =>
            {
                try
                {
                    var grade = await dbContext.Grades.FindAsync(gradeId);

                    if (grade is null)
                    {
                        return Results.NotFound("Grade not found.");
                    }

                    await dbContext.Entry(grade)
                        .Collection(g => g.Citizens).LoadAsync();
                    
                    var citizen = await dbContext.Citizens.FindAsync(citizenId);

                    if (citizen is null)
                    {
                        return Results.NotFound("Citizen not found.");
                    }

                    grade.Citizens.Remove(citizen);
                    await dbContext.SaveChangesAsync();
                    return Results.Ok(grade.ToDTO());
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("RemoveCitizenFromGrade")
            .WithTags("Grade")
            .WithDescription("Remove a citizen from a grade.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapDelete("/{id}", async (int id, GirafDbContext dbContext) =>
            {
                try
                {
                    var grade = await dbContext.Grades.FindAsync(id);

                    if (grade is null)
                    {
                        return Results.NotFound("Grade not found.");
                    }

                    await dbContext.Grades.Where(g => g.Id == grade.Id).ExecuteDeleteAsync();
                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("DeleteGrade")
            .WithTags("Grade")
            .WithDescription("Delete a grade.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);  
        
        return group;
    }
}