using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace GirafAPI.Entities.Pictograms.DTOs;

public record PictogramDTO(
    int Id,
    [Required] int OrganizationId,
    [Required] string PictogramName
    );
