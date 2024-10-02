using GirafAPI.Entities.Users;
using GirafAPI.Entities.Users.DTOs;

namespace GirafAPI.Mapping;

public static class UserMapping
{
    public static GirafUser ToEntity(this CreateUserDTO userDTO)
    {
        return new GirafUser
        {
            UserName = userDTO.UserName,

        };
    }

}
