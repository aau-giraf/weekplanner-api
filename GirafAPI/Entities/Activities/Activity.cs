using GirafAPI.Entities.Pictograms;

namespace GirafAPI.Entities.Activities;

// Data model of one activity in a day
public class Activity
{
    public int Id { get; set; }
    
    public required DateOnly Date { get; set; }
    
    public required string Name { get; set; }
    
    public required string Description { get; set; }
    
    public required TimeOnly StartTime { get; set; }
    
    public required TimeOnly EndTime { get; set; }

    public bool IsCompleted { get; set; }
    
    public Pictogram Pictogram { get; set; }
}