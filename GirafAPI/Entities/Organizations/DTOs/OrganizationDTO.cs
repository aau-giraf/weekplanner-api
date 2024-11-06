using System.ComponentModel.DataAnnotations;
using GirafAPI.Entities.DTOs;
using GirafAPI.Entities.Grades.DTOs;
using GirafAPI.Entities.Resources.DTOs;
using GirafAPI.Entities.Users;

namespace GirafAPI.Entities.Organizations.DTOs;

public record OrganizationDTO(
    int Id,
    [Required] string Name,
    [Required] ICollection<UserDTO> Users,
    ICollection<CitizenDTO>? Citizens,
    ICollection<GradeDTO>? Grades
    );