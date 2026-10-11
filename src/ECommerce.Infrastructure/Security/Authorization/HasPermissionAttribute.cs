using Microsoft.AspNetCore.Authorization;

namespace ECommerce.Infrastructure.Security.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public class HasPermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "PERMISSION_";

    public HasPermissionAttribute(string permission)
        : base(policy: $"{PolicyPrefix}{permission}")
    {
    }
}
