using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GirafAPI.Entities.Grades;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Users;
using GirafAPI.Entities.Invitations;

namespace GirafAPI.Entities.Organizations;

public class Organization
{
    public int Id { get; set; }
    
    [StringLength(100)] public required string Name { get; set; }
    
    public required ICollection<GirafUser> Users { get; set; }
    
    public required ICollection<Grade> Grades { get; set; }
    
    public required ICollection<Citizen> Citizens { get; set; }
    public virtual ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
}