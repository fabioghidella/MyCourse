using Microsoft.AspNetCore.Authorization;

namespace MyCourse.Customizations.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class AuthorizePolicyAttribute : AuthorizeAttribute
{
    // Thanks to this constructor, policies can be provided as Policy objects rather than plain strings.
    // Example: [AuthorizePolicy(Policy.CourseAuthor)]
    public AuthorizePolicyAttribute(params Policy[] policies)
    {
        // Convert the policy names to strings,
        // as required by the Policy property of AuthorizeAttribute.
        // NOTE: ASP.NET Core does not normally allow specifying more than one policy name.
        // This is only possible with custom logic: see the MultiAuthorizationPolicyProvider class.
        Policy = string.Join(",", policies);
    }
}
