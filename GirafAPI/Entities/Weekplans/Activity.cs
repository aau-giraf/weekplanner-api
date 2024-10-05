namespace GirafAPI.Entities.Weekplans;

// Data model of one activity in a day
public class Activity
{
    public int Id { get; set; }
    
    public int DayplanId { get; set; }
    
    public required Dayplan Dayplan { get; set; }
    
    public required string Name { get; set; }
    
    public required string Description { get; set; }
    
    public TimeOnly StartTime { get; set; }
    
    public TimeOnly EndTime { get; set; }
    
    public DateOnly Date { get; set; }
}