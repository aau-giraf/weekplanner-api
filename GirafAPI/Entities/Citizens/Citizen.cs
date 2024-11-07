using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Weekplans;

namespace GirafAPI.Entities.Citizens;

// Data model of Citizens in the database
public class Citizen
{
    public int Id { get; set; }
    
    public required string FirstName { get; set; }
    
    public required string LastName { get; set; }
    
    public required Organization Organization { get; set; }
    
    public ICollection<Activity>? Activities { get; set; }
}