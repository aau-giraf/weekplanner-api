using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.Organizations.DTOs;

public record OrganizationThumbnailDTO(
    int Id,
    [Required] string Name
    );