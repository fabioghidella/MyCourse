using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MyCourse.Customizations.Identity;

public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
{
    public CustomClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim("FullName", user.FullName));

        // In several parts of the application we use the "CourseAuthor" policy,
        // which triggers a database query to verify whether the user's ID matches
        // the author ID of the course being accessed.
        // This query could be avoided by adding a custom claim containing the IDs of the user's courses.
        // After retrieving those IDs from the database via the course service, they could be added as a claim:
        // identity.AddClaim(new Claim("AuthorOfCourses", "5,7,22"));

        return identity;
    }
}
