using Microsoft.AspNetCore.Authorization;

namespace MyCourse.Customizations.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class AuthorizeRoleAttribute : AuthorizeAttribute
{
    // Thanks to this constructor, roles can be provided as Role enum values rather than plain strings.
    // Example with 1 role: [AuthorizeRole(Role.Teacher)]
    // Example with 2 roles: [AuthorizeRole(Role.Teacher, Role.Administrator)]
    public AuthorizeRoleAttribute(params Role[] roles)
    {
        // Convert them to strings and join with a comma,
        // as required by the Roles property of the Authorize attribute.
        Roles = string.Join(",", roles.Select(role => role.ToString()));
    }
}
