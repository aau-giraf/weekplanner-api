using System.Security.Claims;
using GirafAPI.Data;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace GirafAPI.Authorization;

public class OrgAdminRequirement : IAuthorizationRequirement;

public class OrgAdminAuthorizationHandler : AuthorizationHandler<OrgAdminRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<GirafUser> _userManager;

    public OrgAdminAuthorizationHandler(IHttpContextAccessor httpContextAccessor, UserManager<GirafUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrgAdminRequirement requirement)
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
            .Where(c => c.Type == "OrgAdmin")
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