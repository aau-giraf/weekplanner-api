using GirafAPI.Data;
using GirafAPI.Entities.Pictograms;
using GirafAPI.Entities.Pictograms.DTOs;
using GirafAPI.Mapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Endpoints;

public static class PictogramEndpoints
{
    public static RouteGroupBuilder MapPictogramEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("pictograms");

        // Can't use a DTO here since for the endpoint to work correctly with images, the image and the dto must both be multipart/form-data
        // but minimal apis can't map from multipart/form-data to a record DTO, only from application/json
        // therefore we use manual binding
        group.MapPost("/", async ([FromForm] IFormFile image, [FromForm] int? organizationId, [FromForm] string pictogramName, GirafDbContext context) =>
            {
                if (image.Length == 0)
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

                var fileExtension = Path.GetExtension(image.FileName);
                var url = Path.Combine("images", "pictograms", organizationId.ToString(), $"{pictogramName}{fileExtension}");
                Pictogram pictogram = createPictogramDTO.ToEntity(url);
                //Ensure the directory exists
                var directoryPath = Path.GetDirectoryName(Path.Combine("wwwroot", url));
                if (directoryPath != null)
                {
                    Directory.CreateDirectory(directoryPath);
                }
                await using var stream = new FileStream(Path.Combine("wwwroot", url), FileMode.Create);
                await image.CopyToAsync(stream);
                try
                {
                    context.Pictograms.Add(pictogram);
                    await context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Results.BadRequest("Failed to upload pictogram");
                }
                return Results.Ok(pictogram.PictogramUrl);

            })
            .DisableAntiforgery()
            .WithName("CreatePictogram")
            .WithDescription("Creates a pictogram")
            .RequireAuthorization("OrganizationMember")
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

                return Results.Ok(pictogram.ToDTO());
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

        group.MapGet("/organizationId:int", async (int organizationId, int currentPage, int pageSize, GirafDbContext dbContext) =>
            {
              try
              {
                  var skip = (currentPage - 1) * pageSize;

                  var pictograms = await dbContext.Pictograms
                      .Where(p => p.OrganizationId == organizationId || p.OrganizationId == null)
                      .Skip(skip)  
                      .Take(pageSize)  
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
            .RequireAuthorization("OrganizationMember")
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
                    if (File.Exists(Path.Combine("wwwroot", "images", pictogram.PictogramUrl)))
                    {
                        File.Delete(Path.Combine("wwwroot", "images", pictogram.PictogramUrl));
                    }
                    dbContext.Pictograms.Remove(pictogram);
                    await dbContext.SaveChangesAsync();
                    return Results.Ok("Pictogram deleted");
                }
                return Results.NotFound("Pictogram not found");
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
