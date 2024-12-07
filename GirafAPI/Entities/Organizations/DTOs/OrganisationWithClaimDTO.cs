using System.ComponentModel.DataAnnotations;
using GirafAPI.Entities.Citizens.DTOs;
using GirafAPI.Entities.DTOs;
using GirafAPI.Entities.Grades.DTOs;
using GirafAPI.Entities.Users.DTOs;

namespace GirafAPI.Entities.Organizations.DTOs;

public record OrganisationWithClaimDTO(
    int Id,
    [Required] string Name,
    [Required] ICollection<UserWithRoleDTO> Users,
    ICollection<CitizenDTO>? Citizens,
    ICollection<GradeDTO>? Grades,
    string UserRole
);