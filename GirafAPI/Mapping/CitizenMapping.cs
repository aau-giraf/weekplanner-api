using GirafAPI.Entities.Resources;
using GirafAPI.Entities.Resources.DTOs;
using GirafAPI.Entities.Weekplans;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace GirafAPI.Mapping;

// Static methods for converting between entities and DTOs
public static class CitizenMapping
{
    public static async Task<Citizen> ToEntityAsync(this CreateCitizenDTO citizenDTO, UserManager<GirafUser> userManager)
    {
        // Create the user account
        var user = new GirafUser
        {
            UserName = citizenDTO.Username,
        };
        var result = await userManager.CreateAsync(user, citizenDTO.Password);

        if (!result.Succeeded)
        {
            throw new Exception("Failed to create user account");
        }

        // Create the citizen entity
        return new Citizen
        {
            FirstName = citizenDTO.FirstName,
            LastName = citizenDTO.LastName,
            Weekplan = new Weekplan(),
            UserId = user.Id
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