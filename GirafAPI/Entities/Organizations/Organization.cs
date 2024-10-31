using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GirafAPI.Entities.Resources;
using GirafAPI.Entities.Users;

namespace GirafAPI.Entities.Organizations;

public class Organization
{
    public int Id { get; set; }
    
    [StringLength(100)] public required string Name { get; set; }
    
    public required ICollection<GirafUser> Users { get; set; }
    
    public required ICollection<Citizen> Citizens { get; set; }
}