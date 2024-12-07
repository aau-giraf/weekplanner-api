using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace GirafAPI.Utils;

public static class ClaimUtils
{
    private static readonly Dictionary<string, int> RoleHierarchy = new()
    {
        { "OrgMember", 3 },
        { "OrgAdmin", 2 },
        { "OrgOwner", 1 }
    };

    public static IdentityUserClaim<string> GetHighestClaim(List<IdentityUserClaim<string>> claims)
    {
        return claims.OrderBy(uc => RoleHierarchy.TryGetValue(uc.ClaimType, out var value) ? value : 0).First();
    }
}