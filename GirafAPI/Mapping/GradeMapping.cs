using GirafAPI.Entities.Grades;
using GirafAPI.Entities.Grades.DTOs;
using GirafAPI.Entities.Resources.DTOs;

namespace GirafAPI.Mapping;

public static class GradeMapping
{
    public static GradeDTO ToDTO(this Grade grade)
    {
        var citizens = new List<CitizenDTO>();
        if (grade.Citizens is not null)
        {
            foreach (var citizen in grade.Citizens)
            {
                citizens.Add(citizen.ToDTO());
            }
        }
        
        return new GradeDTO(
            grade.Id,
            grade.Name,
            citizens
        );
    }
}