using System.Security.Claims;
using ECommerce.Infrastructure.Security.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace ECommerce.Tests;

public class PermissionAuthorizationTests
{
    [Fact]
    public async Task HandleRequirementAsync_ShouldSucceed_WhenUserHasMatchingPermissionClaim()
    {
        // Arrange
        var handler = new PermissionAuthorizationHandler();
        var requirement = new PermissionRequirement("product.manage");

        var claims = new[]
        {
            new Claim("permission", "product.view"),
            new Claim("permission", "product.manage")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldFail_WhenUserLacksPermissionClaim()
    {
        // Arrange
        var handler = new PermissionAuthorizationHandler();
        var requirement = new PermissionRequirement("order.manage");

        var claims = new[]
        {
            new Claim("permission", "product.view")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}
