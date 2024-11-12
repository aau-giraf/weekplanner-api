using GirafAPI.Entities.DTOs;
using GirafAPI.Entities.Users;
using GirafAPI.Entities.Users.DTOs;
using Microsoft.AspNetCore.Identity;

namespace GirafAPI.Mapping;

public static class UserMapping
{
    public static GirafUser ToEntity(this CreateUserDTO userDTO)
    {
        return new GirafUser
        {
            Email = userDTO.Email,
            
            FirstName = userDTO.FirstName,
            
            LastName = userDTO.LastName,
            
            UserName = userDTO.Email,  

        };
    }

    public static GirafUser ToEntity(this UpdateUserDTO user)
    {
        return new GirafUser
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
        };
    }

    public static UserDTO ToDTO(this GirafUser user)
    {
        return new UserDTO(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName
        );
    }
}
