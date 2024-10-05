using GirafAPI.Entities.Weekplans;
using GirafAPI.Entities.Weekplans.DTOs;

namespace GirafAPI.Mapping;

public static class DayplanMapping
{
    public static Dayplan ToEntity(this CreateDayplanDTO dayplan)
    {
        return new Dayplan
        {
            CitizenId = dayplan.CitizenId,
            Date = DateOnly.Parse(dayplan.Date),
            Activities = new List<Activity>()
        };
    }
}