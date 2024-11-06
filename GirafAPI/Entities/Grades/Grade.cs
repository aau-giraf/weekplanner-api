using GirafAPI.Entities.Resources;

namespace GirafAPI.Entities.Grades;

public class Grade
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public required string Name { get; set; }
    public required ICollection<Citizen> Citizens { get; set; }
}