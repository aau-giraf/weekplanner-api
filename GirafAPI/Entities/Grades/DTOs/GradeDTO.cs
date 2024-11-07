using GirafAPI.Entities.Citizens.DTOs;

namespace GirafAPI.Entities.Grades.DTOs;

public record GradeDTO(
    int Id,
    string Name,
    List<CitizenDTO> Citizens);