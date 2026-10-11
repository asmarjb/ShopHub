using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ECommerce.Infrastructure.Security.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // 1. SuperAdmin role has universal access
        if (context.User.IsInRole("SuperAdmin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // 2. Check if user has the specific permission claim
        var hasPermission = context.User.Claims.Any(c =>
            c.Type == "permission" &&
            c.Value.Equals(requirement.Permission, StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
