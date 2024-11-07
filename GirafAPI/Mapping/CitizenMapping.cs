using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Resources;
using GirafAPI.Entities.Resources.DTOs;
using GirafAPI.Entities.Weekplans;

namespace GirafAPI.Mapping;

// Static methods for converting between entities and DTOs
public static class CitizenMapping
{
    public static Citizen ToEntity(this CreateCitizenDTO citizen, Organization organization)
    {
        return new Citizen
        {
            FirstName = citizen.FirstName,
            LastName = citizen.LastName,
            Activities = new List<Activity>(),
            Organization = organization
        };
    }

    public static Citizen ToEntity(this UpdateCitizenDTO citizen, int id, ICollection<Activity> activities, Organization organization)
    {
        return new Citizen
        {
            Id = id,
            FirstName = citizen.FirstName,
            LastName = citizen.LastName,
            Activities = activities,
            Organization = organization
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