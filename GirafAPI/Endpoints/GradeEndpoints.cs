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
        
        group.MapGet("/{orgId}/{gradeId}", async (int orgId, int gradeId, GirafDbContext dbContext) =>
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
        .RequireAuthorization("OrganizationMember")
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
        .RequireAuthorization("OrganizationMember")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapPost("/{orgId}", async (int orgId, CreateGradeDTO newGrade, GirafDbContext dbContext) =>
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
        .RequireAuthorization("OrganizationAdmin")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapPut("/{orgId}/{gradeId}/change-name", async (int orgId, int gradeId, string newName, GirafDbContext dbContext) =>
        {
            try
            {
                var grade = await dbContext.Grades.FindAsync(gradeId);

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
        .RequireAuthorization("OrganizationAdmin")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapPut("/{orgId}/{gradeId}/add-citizens", async (int orgId, int gradeId, List<int> citizenIds, GirafDbContext dbContext) =>
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
                    
                    var citizens = await dbContext.Citizens
                        .Where(c => citizenIds.Contains(c.Id))
                        .ToListAsync();
                    
                    var missingCitizenIds = citizenIds.Except(citizens.Select(c => c.Id)).ToList();
                    if (missingCitizenIds.Any())
                    {
                        return Results.NotFound("Some citizens not found. Please refresh the page.");
                    }
                    
                    var alreadyAssignedCitizens = new List<string>();
                    foreach (var citizen in citizens)
                    {
                        if (grade.Citizens.Contains(citizen))
                        {
                            alreadyAssignedCitizens.Add($"{grade.Name}: {citizen.FirstName} {citizen.LastName}");
                        }
                        else
                        {
                            grade.Citizens.Add(citizen);
                        }
                    }
                    
                    if (alreadyAssignedCitizens.Any())
                    {
                        return Results.BadRequest($"Citizens already assigned to {string.Join(", ", alreadyAssignedCitizens)}.");
                    }
                    
                    await dbContext.SaveChangesAsync();
                    return Results.Ok(grade.ToDTO());
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("AddCitizensToGrade")
            .WithTags("Grade")
            .WithDescription("Add one or more citizens to a grade.")
            .RequireAuthorization("OrganizationMember")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapPut("/{orgId}/{gradeId}/remove-citizens", async (int orgId, int gradeId, List<int> citizenIds, GirafDbContext dbContext) =>
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
                    
                    var citizens = await dbContext.Citizens
                        .Where(c => citizenIds.Contains(c.Id))
                        .ToListAsync();
                    
                    var missingCitizenIds = citizenIds.Except(citizens.Select(c => c.Id)).ToList();
                    if (missingCitizenIds.Any())
                    {
                        return Results.NotFound("Some citizens not found. Please refresh the page.");
                    }
                    
                    var alreadyRemovedCitizens = new List<string>();
                    foreach (var citizen in citizens)
                    {
                        if (!grade.Citizens.Contains(citizen))
                        {
                            alreadyRemovedCitizens.Add($"{citizen.FirstName} {citizen.LastName}");
                        }
                        else
                        {
                            grade.Citizens.Remove(citizen);
                        }
                    }
                    if (alreadyRemovedCitizens.Any())
                    {
                        return Results.BadRequest($"The following citizens were already removed from {grade.Name}: {string.Join(", ", alreadyRemovedCitizens)}.");
                    }
                    
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
            .WithDescription("Remove one or more citizens from a grade.")
            .RequireAuthorization("OrganizationMember")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
        
        group.MapDelete("/{orgId}/{id}", async (int orgId, int id, GirafDbContext dbContext) =>
            {
                try
                {
                    // Find the grade by ID
                    var grade = await dbContext.Grades
                        .Include(g => g.Citizens) // Assuming there's a navigation property for citizens
                        .FirstOrDefaultAsync(g => g.Id == id);

                    if (grade is null)
                    {
                        return Results.NotFound("Grade not found.");
                    }

                    // Clear the citizens list if it exists
                    if (grade.Citizens != null)
                    {
                        grade.Citizens.Clear(); // Empties the list of citizens
                        await dbContext.SaveChangesAsync(); // Save the change
                    }

                    // Now delete the grade
                    dbContext.Grades.Remove(grade);
                    await dbContext.SaveChangesAsync();

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
            .RequireAuthorization("OrganizationAdmin")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
        
        return group;
    }
}
