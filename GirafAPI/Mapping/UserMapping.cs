using GirafAPI.Data;
using GirafAPI.Entities.DTOs;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using GirafAPI.Entities.Users.DTOs;
using GirafAPI.Utils;

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

    public static UserWithRoleDTO ToUserWithClaims(this GirafUser user, Organization organization,
        GirafDbContext dbContext)
    {
        var claimList = dbContext.UserClaims
            .Where(uc => uc.UserId == user.Id && uc.ClaimValue == organization.Id.ToString())
            .AsEnumerable()
            .ToList();

        var highestClaim = ClaimUtils.GetHighestClaim(claimList);

        return new UserWithRoleDTO(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            highestClaim.ClaimType
        );
    }
}