using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.Resources.DTOs;

public record CreatePictogramDTO(
    [Required] IFormFile Image,
    [Required] string PictogramName
    );
