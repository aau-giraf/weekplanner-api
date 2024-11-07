using System.ComponentModel.DataAnnotations;

namespace GirafAPI.Entities.Organizations.DTOs;

public record OrganizationNameOnlyDTO(
    int Id,
    [Required] string Name
    );