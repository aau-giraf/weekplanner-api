using GirafAPI.Entities.Weekplans;
using GirafAPI.Entities.Weekplans.DTOs;

namespace GirafAPI.Mapping;

public static class ActivityMapping
{
    public static Activity ToEntity(this CreateActivityDTO activityDto)
    {
        return new Activity
        {
            CitizenId = activityDto.CitizenId,
            Date = DateOnly.Parse(activityDto.Date),
            Name = activityDto.Name,
            Description = activityDto.Description,
            StartTime = TimeOnly.Parse(activityDto.StartTime),
            EndTime = TimeOnly.Parse(activityDto.EndTime)
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
            EndTime = TimeOnly.Parse(activityDto.EndTime)
        };
    }

    public static ActivityDTO ToDTO(this Activity activity)
    {
        return new ActivityDTO(
            activity.Id,
            activity.CitizenId,
            activity.Date.ToString(),
            activity.Name,
            activity.Description,
            activity.StartTime.ToString(),
            activity.EndTime.ToString()
        );
    }
}