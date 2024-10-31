using GirafAPI.Data;
using GirafAPI.Entities.Resources;
using GirafAPI.Entities.Resources.DTOs;
using GirafAPI.Mapping;
using Microsoft.AspNetCore.Mvc;

namespace GirafAPI.Endpoints;

public static class PictogramEndpoints
{
    public static RouteGroupBuilder MapPictogramEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("pictograms").AllowAnonymous();

        group.MapPost("/{orgId:int}", async (int orgId, [FromForm] CreatePictogramDTO pictogramDTO) =>
            {
                if (pictogramDTO.Image is null || pictogramDTO.Image.Length == 0)
                {
                    return Results.BadRequest("Image file is required");
                }

                if (string.IsNullOrEmpty(pictogramDTO.PictogramName))
                {
                    return Results.BadRequest("Pictogram name is required");
                }

                Pictogram pictogram = pictogramDTO.ToEntity();
                //Create a filepath where the name of the image is a unique id generated when the pictogram becomes an entity
                var filePath = Path.Combine($"pictograms/{orgId}", $"{pictogram.ImageId}.jpg");
                //Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                await using var stream = new FileStream(filePath, FileMode.Create);
                await pictogramDTO.Image.CopyToAsync(stream);

                return Results.Ok();

            })
            .WithName("CreatePictogram")
            .WithDescription("Creates a pictogram")
            .WithTags("Pictograms")
            .Accepts<CreatePictogramDTO>("multipart/form-data")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);


        group.MapGet("/{pictogramId:int}", async (int pictogramId, GirafDbContext dbContext) =>
            {
              try
              {
                Pictogram? pictogram = await dbContext.Pictograms.FindAsync(pictogramId);

                return pictogram is null ? Results.NotFound("Pictogram not found") : Results.Ok(pictogram.ToDTO);
              }
              catch (Exception)
              {
                return Results.Problem("An error has occured while retrieving the activity.", statusCode: StatusCodes.Status500InternalServerError);
              }
            })
            .WithName("GetPictogramById")
            .WithDescription("Gets a specific pictogram by Id.")
            .WithTags("Pictograms")
            .Produces<PictogramDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);


        group.MapDelete("/{pictogramId:int}", async (int pictogramId, GirafDbContext dbContext) =>
            {
              try
              {
                Pictogram? pictogram = await dbContext.Pictograms.FindAsync(pictogramId);
                if (pictogram is not null)
                {
                  dbContext.Pictograms.Remove(pictogram);
                  return Results.Ok("Pictogram deleted");
                }
                else
                {
                  return Results.NotFound("Pictogram not found");
                }
              }
              catch (Exception)
              {
                return Results.Problem("An error has occured while retrieving the pictogram slated for deletion.", statusCode: StatusCodes.Status500InternalServerError);
              }
            })
            .WithName("DeletePictogram")
            .WithDescription("Deletes a pictogram by Id.")
            .WithTags("Pictograms")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
        
        
        return group;
    }
    
}
