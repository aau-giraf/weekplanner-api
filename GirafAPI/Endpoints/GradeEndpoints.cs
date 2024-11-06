using GirafAPI.Data;
using GirafAPI.Entities.Grades.DTOs;

namespace GirafAPI.Endpoints;

public static class GradeEndpoints
{
    public static RouteGroupBuilder MapGradeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("grades");
        
        group.MapPost("/", async (CreateGradeDTO newGrade, GirafDbContext dbContext) =>
        {
            try
            {
                
            }
        })
        .WithName("CreateGrade");    
    }
}