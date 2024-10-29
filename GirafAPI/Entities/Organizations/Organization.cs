using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GirafAPI.Entities.Resources;
using GirafAPI.Entities.Users;

namespace GirafAPI.Entities.Organizations;

public class Organization
{
    public int Id { get; set; }
    
    [StringLength(100)] public required string Name { get; set; }
    
    [NotMapped] public required ICollection<GirafUser> Admins { get; set; }
    
    public required ICollection<GirafUser> Users { get; set; }
    
    public ICollection<Citizen>? Citizens { get; set; }
}