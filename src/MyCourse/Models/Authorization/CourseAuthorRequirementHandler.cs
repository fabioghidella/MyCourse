using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace MyCourse.Models.Authorization;

public class CourseAuthorRequirementHandler : AuthorizationHandler<CourseAuthorRequirement>
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ICachedCourseService courseService;
    private readonly ILessonService lessonService;

    public CourseAuthorRequirementHandler(IHttpContextAccessor httpContextAccessor, ICachedCourseService courseService, ILessonService lessonService)
    {
        this.courseService = courseService;
        this.lessonService = lessonService;
        this.httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                 CourseAuthorRequirement requirement)
    {
        string userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        int courseId;
        if (context.Resource is int)
        {
            courseId = (int)context.Resource;
        }
        else
        {
            int id = Convert.ToInt32(httpContextAccessor.HttpContext.Request.RouteValues["id"]);
            if (id == 0)
            {
                context.Fail();
                return;
            }

            // Which controller are we trying to access?
            switch (httpContextAccessor.HttpContext.Request.RouteValues["controller"].ToString().ToLowerInvariant())
            {
                // This is a lesson. Retrieve the id of the course it belongs to.
                case "lessons":
                    courseId = (await lessonService.GetLessonAsync(id)).CourseId;
                    break;

                // The id belongs to a course directly.
                case "courses":
                    courseId = id;
                    break;

                default:
                    // Unsupported controller
                    context.Fail();
                    return;
            }
        }

        string authorId = await courseService.GetCourseAuthorIdAsync(courseId);
        if (userId == authorId)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
