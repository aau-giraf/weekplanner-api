using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GirafAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Authorization;

public class InvitationRecipientOrAdminRequirement : IAuthorizationRequirement;

public class InvitationRecipientOrAdminHandler : AuthorizationHandler<InvitationRecipientOrAdminRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly GirafDbContext _dbContext;

    public InvitationRecipientOrAdminHandler(IHttpContextAccessor httpContextAccessor, GirafDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        InvitationRecipientOrAdminRequirement requirement)
    {
        Console.WriteLine("[HANDLER] Running InvitationRecipientOrAdminHandler...");

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            Console.WriteLine("[HANDLER] httpContext is null, failing authorization.");
            // Just return here; this will not meet the requirement, thus authorization will fail.
            return;
        }

        // Extract the 'id' route value
        var idValue = httpContext.Request.RouteValues["id"];
        Console.WriteLine($"[HANDLER] RouteValue[id] = {idValue}");
        if (!int.TryParse(idValue?.ToString(), out int invitationId))
        {
            Console.WriteLine("[HANDLER] Could not parse 'id' route value.");
            // No valid ID means we can't find an invitation; let the endpoint handle 404.
            // Succeed so endpoint can run and return NotFound.
            context.Succeed(requirement);
            return;
        }

        // Load the invitation
        var invitation = await _dbContext.Invitations.Include(i => i.Organization)
            .FirstOrDefaultAsync(i => i.Id == invitationId);

        if (invitation == null)
        {
            Console.WriteLine("[HANDLER] Invitation not found. Letting endpoint handle the response.");
            // Succeed here, so the endpoint runs and returns 404.
            context.Succeed(requirement);
            return;
        }

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine($"[HANDLER] Found NameIdentifier claim: {userId}");

        if (userId == null)
        {
            Console.WriteLine("[HANDLER] No NameIdentifier claim found, cannot authorize.");
            context.Succeed(requirement);
            return; // fails authorization by not succeeding
        }

        // Check if user is the receiver
        Console.WriteLine($"[HANDLER] Invitation.ReceiverId={invitation.ReceiverId}, UserId={userId}");
        if (invitation.ReceiverId == userId)
        {
            Console.WriteLine("[HANDLER] User is the receiver, succeeding.");
            context.Succeed(requirement);
            return;
        }

        // Check if user is admin of the org
        Console.WriteLine("[HANDLER] Checking if user is org admin...");
        var isAdmin = context.User.Claims.Any(c => c.Type == "OrganizationAdmin" && c.Value == invitation.OrganizationId.ToString());
        if (isAdmin)
        {
            Console.WriteLine("[HANDLER] User is org admin, succeeding.");
            context.Succeed(requirement);
            return;
        }

        Console.WriteLine("[HANDLER] User is neither receiver nor admin, failing authorization.");
        context.Fail();
    }
}
