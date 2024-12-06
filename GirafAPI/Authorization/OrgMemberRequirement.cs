using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace GirafAPI.Authorization;

public class OrgMemberRequirement : IAuthorizationRequirement;

public class OrgMemberAuthorizationHandler : AuthorizationHandler<OrgMemberRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<GirafUser> _userManager;

    public OrgMemberAuthorizationHandler(IHttpContextAccessor httpContextAccessor, UserManager<GirafUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrgMemberRequirement requirement)
    {
        
        var userId = _userManager.GetUserId(_httpContextAccessor.HttpContext.User);
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            context.Fail();
            return;
        }

        var claims = await _userManager.GetClaimsAsync(user);
        
        var orgIds = claims
            .Where(c => c.Type == "OrgMember")
            .Select(c => c.Value)
            .ToList();
        
        var httpContext = _httpContextAccessor.HttpContext;
        var orgIdInUrl = httpContext.Request.RouteValues["orgId"];

        if (orgIds.Contains(orgIdInUrl))
        {
            context.Succeed(requirement);
            return;
        }
        
        context.Fail();
    }
}