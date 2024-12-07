// Authorization/RespondInvitationHandler.cs
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GirafAPI.Data;

namespace GirafAPI.Authorization;

public class RespondInvitationRequirement : IAuthorizationRequirement;

public class RespondInvitationHandler : AuthorizationHandler<RespondInvitationRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly GirafDbContext _dbContext;

    public RespondInvitationHandler(IHttpContextAccessor httpContextAccessor, GirafDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RespondInvitationRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            context.Fail();
            return;
        }

        // Extract the invitation ID from the route
        if (!int.TryParse(httpContext.Request.RouteValues["id"]?.ToString(), out int invitationId))
        {
            context.Succeed(requirement);
            return;
        }

        // Retrieve the invitation from the database
        var invitation = await _dbContext.Invitations.FindAsync(invitationId);
        if (invitation == null)
        {
            context.Succeed(requirement);
            return;
        }

        // Retrieve the user ID from the claims
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            context.Succeed(requirement);
            return;
        }

        // Check if the current user is the receiver of the invitation
        if (invitation.ReceiverId == userId)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
