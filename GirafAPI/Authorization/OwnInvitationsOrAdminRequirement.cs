// Authorization/UserOwnInvitationsOrAdminHandler.cs
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GirafAPI.Authorization;

public class OwnInvitationOrAdminRequirement : IAuthorizationRequirement;

public class OwnInvitationOrAdminHandler(IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<OwnInvitationOrAdminRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnInvitationOrAdminRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Extract the userId from the route
        var userIdInRoute = httpContext.Request.RouteValues["userId"]?.ToString();
        if (string.IsNullOrEmpty(userIdInRoute))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var currentUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if the user is requesting their own invitations
        if (currentUserId == userIdInRoute)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Alternatively, check if the user is an admin
        var isAdmin = context.User.IsInRole("OrganizationAdmin");

        if (isAdmin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        context.Fail();
        return Task.CompletedTask;
    }
}