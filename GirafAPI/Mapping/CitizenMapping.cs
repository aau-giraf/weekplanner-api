using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Citizens.DTOs;
using GirafAPI.Entities.Weekplans;

namespace GirafAPI.Mapping;

// Static methods for converting between entities and DTOs
public static class CitizenMapping
{
    public static Citizen ToEntity(this CreateCitizenDTO citizen)
    {
        return new Citizen
        {
            FirstName = citizen.FirstName,
            LastName = citizen.LastName,
            Activities = new List<Activity>()
        };
    }

    public static Citizen ToEntity(this UpdateCitizenDTO citizen, int id, ICollection<Activity> activities)
    {
        return new Citizen
        {
            Id = id,
            FirstName = citizen.FirstName,
            LastName = citizen.LastName,
            Activities = activities
        };
    }

    public static CitizenDTO ToDTO(this Citizen citizen)
    {
        return new CitizenDTO(
            citizen.Id,
            citizen.FirstName,
            citizen.LastName
        );
    }
}