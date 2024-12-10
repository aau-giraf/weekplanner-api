using GirafAPI.Data;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace GirafAPI.Authorization;

public class OrgOwnerRequirement : IAuthorizationRequirement
{
    
}

public class OrgOwnerAuthorizationHandler : AuthorizationHandler<OrgOwnerRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<GirafUser> _userManager;
    private readonly GirafDbContext _dbContext;

    public OrgOwnerAuthorizationHandler(IHttpContextAccessor httpContextAccessor, 
                                        UserManager<GirafUser> userManager,
                                        GirafDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrgOwnerRequirement requirement)
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
            .Where(c => c.Type == "OrgOwner")
            .Select(c => c.Value)
            .ToList();
        
        var httpContext = _httpContextAccessor.HttpContext;
        var orgIdInUrl = httpContext.Request.RouteValues["orgId"];
        Organization organization;
        
        if (orgIdInUrl is string) // The test environment sends route values as strings
        {
            int orgId = Convert.ToInt32(orgIdInUrl);
            organization = await _dbContext.Organizations.FindAsync(orgId);
        }
        else
        {
            organization = await _dbContext.Organizations.FindAsync(orgIdInUrl);
        }
        
        if (organization == null)
        {
            // Succeed and let the endpoint return NotFound
            context.Succeed(requirement);
            return;
        }

        if (orgIds.Contains(orgIdInUrl))
        {
            context.Succeed(requirement);
            return;
        }
        
        context.Fail();
    }
}