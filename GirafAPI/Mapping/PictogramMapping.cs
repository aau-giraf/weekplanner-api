using GirafAPI.Entities.Resources;
using GirafAPI.Entities.Resources.DTOs;

namespace GirafAPI.Mapping;

public static class PictogramMapping
{
    public static Pictogram ToEntity(this CreatePictogramDTO pictogramDto)
    {
        return new Pictogram
        {
            ImageId = Guid.NewGuid(),
            PictogramName = pictogramDto.PictogramName,
        };
    }

    public static PictogramDTO ToDTO(this Pictogram pictogram, IFormFile imageFile)
    {
        return new PictogramDTO(
            pictogram.Id,
            Image: imageFile,
            pictogram.PictogramName
        );
    }
}