using Microsoft.AspNetCore.Authorization;

namespace GirafAPI.Authorization;

public class OrgMemberRequirement : IAuthorizationRequirement;

public class OrgMemberAuthorizationHandler : AuthorizationHandler<OrgMemberRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrgMemberAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrgMemberRequirement requirement)
    {
        var claims = context.User;
        var orgIds = claims.Claims
                                .Where(c => c.Type == "OrgMember")
                                .Select(c => c.Value)
                                .ToList();
        
        var httpContext = _httpContextAccessor.HttpContext;
        var orgIdInUrl = httpContext.Request.RouteValues["orgId"];

        if (orgIds.Contains(orgIdInUrl))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        
        context.Fail();
        return Task.CompletedTask;
    }
}