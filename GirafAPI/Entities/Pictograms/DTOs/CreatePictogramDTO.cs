using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.Pictograms.DTOs;

public record CreatePictogramDTO(
    [Required] IFormFile Image,
    [Required] int OrganizationId,
    [Required] string PictogramName
    );
