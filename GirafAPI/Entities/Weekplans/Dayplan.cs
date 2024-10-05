using System.ComponentModel.DataAnnotations;
using GirafAPI.Entities.Resources;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Entities.Weekplans;

// Data model of one day in the weekplan
[PrimaryKey(nameof(CitizenId), nameof(Date))]
public class Dayplan
{
    [Key]
    public int Id { get; set; }
    
    public int CitizenId { get; set; }
    
    public DateOnly Date { get; set; }
    
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}