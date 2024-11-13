using GirafAPI.Data;
using GirafAPI.Entities.Pictograms;
using GirafAPI.Entities.Pictograms.DTOs;
using GirafAPI.Mapping;
using GirafAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Endpoints;

public static class PictogramEndpoints
{
    public static RouteGroupBuilder MapPictogramEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("pictograms").AllowAnonymous();

        // Can't use a DTO here since for the endpoint to work correctly with images, the image and the dto must both be multipart/form-data
        // but minimal apis can't map from multipart/form-data to a record DTO, only from application/json
        // therefore we use manual binding
        group.MapPost("/", async ([FromForm] IFormFile image, [FromForm] int? organizationId, [FromForm] string pictogramName, GirafDbContext context) =>
            {
                if (image is null || image.Length == 0)
                {
                    return Results.BadRequest("Image file is required");
                }

                if (organizationId is null)
                {
                    return Results.BadRequest("Organization id is required");
                }

                if (pictogramName.Length == 0)
                {
                    return Results.BadRequest("Pictogram name is required");
                }

                CreatePictogramDTO createPictogramDTO = new CreatePictogramDTO(organizationId.GetValueOrDefault(), pictogramName);
                Pictogram pictogram = createPictogramDTO.ToEntity();
                //Create a filepath where the name of the image is a unique id generated when the pictogram becomes an entity
                var filePath = Path.Combine("/app/pictograms", pictogram.OrganizationId.ToString(), $"{pictogram.ImageId}.jpg");
                //Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                await using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);
                try
                {
                    context.Pictograms.Add(pictogram);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Results.BadRequest("Failed to upload pictogram");
                }
                return Results.Ok();

            })
            .DisableAntiforgery()
            .WithName("CreatePictogram")
            .WithDescription("Creates a pictogram")
            .WithTags("Pictograms")
            .Accepts<IFormFile>("multipart/form-data")
            .Accepts<CreatePictogramDTO>("multipart/form-data")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);


        group.MapGet("/{pictogramId:int}", async (int pictogramId, GirafDbContext dbContext) =>
            {
              try
              {
                Pictogram? pictogram = await dbContext.Pictograms.FindAsync(pictogramId);

                if (pictogram is null)
                {
                    return Results.NotFound("Pictogram not found");
                }

                var filePath = Path.Combine("/app/pictograms", pictogram.OrganizationId.ToString(), $"{pictogram.ImageId}.jpg");

                if (!File.Exists(filePath))
                {
                  return Results.NotFound("File not found");
                }

                byte[] fileBytes = await File.ReadAllBytesAsync(filePath);

                return Results.File(fileBytes, "application/octet-stream", pictogram.PictogramName);
              }
              catch (Exception)
              {
                return Results.Problem("An error has occured while retrieving the pictogram.", statusCode: StatusCodes.Status500InternalServerError);
              }
            })
            .WithName("GetPictogramById")
            .WithDescription("Gets a specific pictogram by Id.")
            .WithTags("Pictograms")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("/organizationId:int", async (int organizationId, GirafDbContext dbContext) =>
            {
              try
              {
                var pictograms = await dbContext.Pictograms
                    .Where(p => p.OrganizationId == organizationId)
                    .Select(p => p.ToDTO())
                    .AsNoTracking()
                    .ToListAsync();

                return Results.Ok(pictograms);
              }
              catch (Exception)
              {
                return Results.Problem("An error occurred while retrieving pictograms.", statusCode: StatusCodes.Status500InternalServerError);
              }
            })
            .WithName("GetPictogramsByOrgId")
            .WithDescription("Gets all the pictograms belonging to the specified organization.")
            .WithTags("Pictograms")
            .Produces<List<PictogramDTO>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
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
