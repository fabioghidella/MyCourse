using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace MyCourse.Models.Authorization;

public class CourseLimitRequirementHandler : AuthorizationHandler<CourseLimitRequirement>
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ICachedCourseService courseService;

    public CourseLimitRequirementHandler(IHttpContextAccessor httpContextAccessor, ICachedCourseService courseService)
    {
        this.courseService = courseService;
        this.httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                         CourseLimitRequirement requirement)
    {
        // 1. Read the user's ID from their identity
        string userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 2. Retrieve from the database the courses created by the user
        int courseCount = await courseService.GetCourseCountByAuthorIdAsync(userId);

        // 3. Check that the course count is less than or equal to the limit
        if (courseCount <= requirement.Limit)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
