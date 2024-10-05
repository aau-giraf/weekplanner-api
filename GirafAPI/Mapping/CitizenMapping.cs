using GirafAPI.Entities.Resources;
using GirafAPI.Entities.Resources.DTOs;
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
            Dayplans = new List<Dayplan>()
        };
    }

    public static Citizen ToEntity(this UpdateCitizenDTO citizen, int id, ICollection<Dayplan> dayplans)
    {
        return new Citizen
        {
            Id = id,
            FirstName = citizen.FirstName,
            LastName = citizen.LastName,
            Dayplans = dayplans
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