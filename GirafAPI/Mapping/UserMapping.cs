using GirafAPI.Entities.DTOs;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using GirafAPI.Entities.Users.DTOs;

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
            
            Organizations = new List<Organization>()

        };
    }

    public static UserDTO ToDTO(this GirafUser user)
    {
        return new UserDTO(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName
        );
    }
}
