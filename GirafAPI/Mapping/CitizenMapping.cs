using GirafAPI.Entities.Resources;
using GirafAPI.Entities.Resources.DTOs;
using GirafAPI.Entities.Weekplans;

namespace GirafAPI.Mapping;

public static class CitizenMapping
{
    public static Citizen ToEntity(this CreateCitizenDTO citizen)
    {
        return new Citizen
        {
            FirstName = citizen.FirstName,
            LastName = citizen.LastName,
            Weekplan = new Weekplan()
        };
    }

    public static Citizen ToEntity(this UpdateCitizenDTO citizen, int id)
    {
        return new Citizen
        {
            Id = id,
            FirstName = citizen.FirstName,
            LastName = citizen.LastName,
            Weekplan = new Weekplan()
        };
    }

    public static CitizenDTO ToDTO(this Citizen citizen)
    {
        return new CitizenDTO(
            citizen.Id,
            citizen.FirstName,
            citizen.LastName,
            citizen.WeekplanId
        );
    }
}