using GirafAPI.Entities.Resources;
using GirafAPI.Entities.Resources.DTOs;
using GirafAPI.Mapping;
using Microsoft.AspNetCore.Mvc;

namespace GirafAPI.Endpoints;

public static class PictogramEndpoints
{
    public static RouteGroupBuilder MapPictogramEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("pictograms");

        group.MapPost("/{orgId}:int", async (int orgId, [FromForm] CreatePictogramDTO dto) =>
            {
                if (dto.Image is null || dto.Image.Length == 0)
                {
                    return Results.BadRequest("Image file is required");
                }

                if (string.IsNullOrEmpty(dto.PictogramName))
                {
                    return Results.BadRequest("Pictogram name is required");
                }

                Pictogram pictogram = dto.ToEntity();
                //Create a filepath where the name of the image is a unique id generated when the pictogram becomes an entity
                var filePath = Path.Combine($"pictograms/{orgId}", $"{pictogram.ImageId}.jpg");
                //Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                await using var stream = new FileStream(filePath, FileMode.Create);
                await dto.Image.CopyToAsync(stream);

                return Results.Ok();

            })
            .WithName("CreatePictogram")
            .WithDescription("Creates a pictogram")
            .WithTags("Pictograms")
            .Accepts<CreatePictogramDTO>("multipart/form-data")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        
        
        return group;
    }
    
}