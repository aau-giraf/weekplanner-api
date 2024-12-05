using GirafAPI.Entities.Activities;
using GirafAPI.Entities.Activities.DTOs;
using GirafAPI.Entities.Pictograms;

namespace GirafAPI.Mapping;

public static class ActivityMapping
{
    public static Activity ToEntity(this CreateActivityDTO activityDto)
    {
        return new Activity
        {
            Date = activityDto.Date,
            StartTime = activityDto.StartTime,
            EndTime = activityDto.EndTime,
            IsCompleted = false
        };
    }

    public static Activity ToEntity(this CreateActivityDTO activityDto, Pictogram pictogram)
    {
        return new Activity
        {
            Date = activityDto.Date,
            StartTime = activityDto.StartTime,
            EndTime = activityDto.EndTime,
            IsCompleted = false,
            Pictogram = pictogram
        };
    }
    
    public static Activity ToEntity(this UpdateActivityDTO activityDto, int id)
    {
        return new Activity
        {
            Id = id,
            Date = activityDto.Date,
            StartTime = activityDto.StartTime,
            EndTime = activityDto.EndTime,
            IsCompleted = activityDto.IsCompleted,
        };
    }

    public static Activity ToEntity(this UpdateActivityDTO activityDto, int id, Pictogram pictogram)
    {
        return new Activity
        {
            Id = id,
            Date = activityDto.Date,
            StartTime = activityDto.StartTime,
            EndTime = activityDto.EndTime,
            IsCompleted = activityDto.IsCompleted,
            Pictogram = pictogram
        };
    }
    public static ActivityDTO ToDTO(this Activity activity)
    {
        return new ActivityDTO(
            activity.Id,
            activity.Date.ToString("yyyy-MM-dd"),
            activity.StartTime.ToString("HH:mm"),
            activity.EndTime.ToString("HH:mm"),
            activity.IsCompleted,
            activity.Pictogram
        );
    }
}
