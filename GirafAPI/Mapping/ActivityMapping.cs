using GirafAPI.Entities.Weekplans;
using GirafAPI.Entities.Weekplans.DTOs;

namespace GirafAPI.Mapping;

public static class ActivityMapping
{
    public static Activity ToEntity(this CreateActivityDTO activityDto, int citizenId)
    {
        return new Activity
        {
            CitizenId = citizenId,
            Date = DateOnly.Parse(activityDto.Date),
            Name = activityDto.Name,
            Description = activityDto.Description,
            StartTime = TimeOnly.Parse(activityDto.StartTime),
            EndTime = TimeOnly.Parse(activityDto.EndTime),
            IsCompleted = false
        };
    }
    
    public static Activity ToEntity(this UpdateActivityDTO activityDto, int id)
    {
        return new Activity
        {
            Id = id,
            CitizenId = activityDto.CitizenId,
            Date = DateOnly.Parse(activityDto.Date),
            Name = activityDto.Name,
            Description = activityDto.Description,
            StartTime = TimeOnly.Parse(activityDto.StartTime),
            EndTime = TimeOnly.Parse(activityDto.EndTime),
            IsCompleted = activityDto.IsCompleted
        };
    }

    public static ActivityDTO ToDTO(this Activity activity)
    {
        return new ActivityDTO(
            activity.Id,
            activity.CitizenId,
            activity.Date.ToString("yyyy-MM-dd"),
            activity.Name,
            activity.Description,
            activity.StartTime.ToString("HH:mm"),
            activity.EndTime.ToString("HH:mm"),
            activity.IsCompleted
        );
    }
}
