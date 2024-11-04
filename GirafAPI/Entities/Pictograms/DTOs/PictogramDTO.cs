using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace GirafAPI.Entities.Pictograms.DTOs;

public record PictogramDTO(
    [Required] int Id,
    [Required] int OrganizationId,
    [Required] IFormFile Image,
    [Required] string PictogramName
    );
