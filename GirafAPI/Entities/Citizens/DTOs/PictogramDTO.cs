using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace GirafAPI.Entities.Resources.DTOs;

public record PictogramDTO(
    [Required] int Id,
    [Required] IFormFile Image,
    [Required] string PictogramName
    );