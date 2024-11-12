using GirafAPI.Entities.Pictograms;
using GirafAPI.Entities.Pictograms.DTOs;
namespace GirafAPI.Mapping;

public static class PictogramMapping
{
    public static Pictogram ToEntity(this CreatePictogramDTO pictogramDto)
    {
        return new Pictogram
        {
            ImageId = Guid.NewGuid(),
            OrganizationId = pictogramDto.OrganizationId,
            PictogramName = pictogramDto.PictogramName,
        };
    }

    public static PictogramDTO ToDTO(this Pictogram pictogram)
    {
        return new PictogramDTO(
            pictogram.Id,
            pictogram.OrganizationId,
            pictogram.PictogramName
        );
    }
}
