using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyCourse.Models.Exceptions.Application;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Services.Infrastructure;
using MyCourse.Models.ViewModels.Courses;

namespace MyCourse.Controllers;

public class CoursesController : Controller
{
    private readonly ICourseService courseService;
    public CoursesController(ICachedCourseService courseService)
    {
        this.courseService = courseService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(CourseListInputModel input)
    {
        ViewData["Title"] = "Course Catalog";
        ListViewModel<CourseViewModel> courses = await courseService.GetCoursesAsync(input);

        CourseListViewModel viewModel = new()
        {
            Courses = courses,
            Input = input
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Pay(int id)
    {
        string paymentUrl = await courseService.GetPaymentUrlAsync(id);
        return Redirect(paymentUrl);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Subscribe(int id, string token)
    {
        CourseSubscribeInputModel inputModel = await courseService.CapturePaymentAsync(id, token);
        await courseService.SubscribeCourseAsync(inputModel);
        TempData["ConfirmationMessage"] = "Thanks for subscribing! Start watching the first lesson now!";
        return RedirectToAction(nameof(Detail), new { id = id });
    }

    [AllowAnonymous]
    public async Task<IActionResult> Detail(int id)
    {
        CourseDetailViewModel viewModel = await courseService.GetCourseAsync(id);
        ViewData["Title"] = viewModel.Title;
        return View(viewModel);
    }

    [Authorize(Roles = nameof(Role.Teacher))]
    public IActionResult Create()
    {
        ViewData["Title"] = "New course";
        CourseCreateInputModel inputModel = new();
        return View(inputModel);
    }

    [HttpPost]
    [Authorize(Roles = nameof(Role.Teacher))]
    public async Task<IActionResult> Create(CourseCreateInputModel inputModel, [FromServices] IAuthorizationService authorizationService, [FromServices] IEmailClient emailClient, [FromServices] IOptionsMonitor<UsersOptions> usersOptions)
    {
        if (ModelState.IsValid)
        {
            try
            {
                CourseDetailViewModel course = await courseService.CreateCourseAsync(inputModel);

                // To keep the controller thin, this block could be moved inside CreateCourseAsync in the application service.
                // However, be careful not to create circular dependencies: if the course service depends on IAuthorizationService
                // and the authorization handler in turn depends on the course service, the DI container will fail to resolve either service.
                AuthorizationResult result = await authorizationService.AuthorizeAsync(User, nameof(Policy.CourseLimit));
                if (!result.Succeeded)
                {
                    await emailClient.SendEmailAsync(usersOptions.CurrentValue.NotificationEmailRecipient, "Course limit threshold exceeded", $"The instructor {User.Identity.Name} has created a high number of courses. Please verify they can manage all of them.");
                }

                TempData["ConfirmationMessage"] = "Your course has been created! Why not fill in the rest of the details?";
                return RedirectToAction(nameof(Edit), new { id = course.Id });
            }
            catch (CourseTitleUnavailableException)
            {
                ModelState.AddModelError(nameof(CourseDetailViewModel.Title), "This title already exists");
            }
        }

        ViewData["Title"] = "New Course";
        return View(inputModel);
    }

    [Authorize(Policy = nameof(Policy.CourseAuthor))]
    [Authorize(Roles = nameof(Role.Teacher))]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Course";
        CourseEditInputModel inputModel = await courseService.GetCourseForEditingAsync(id);
        return View(inputModel);
    }

    [HttpPost]
    [Authorize(Policy = nameof(Policy.CourseAuthor))]
    [Authorize(Roles = nameof(Role.Teacher))]
    public async Task<IActionResult> Edit(CourseEditInputModel inputModel)
    {
        if (ModelState.IsValid)
        {
            try
            {
                CourseDetailViewModel course = await courseService.EditCourseAsync(inputModel);
                TempData["ConfirmationMessage"] = "Changes saved successfully.";
                return RedirectToAction(nameof(Detail), new { id = inputModel.Id });
            }
            catch (CourseImageInvalidException)
            {
                ModelState.AddModelError(nameof(CourseEditInputModel.Image), "The selected image is not valid");
            }
            catch (CourseTitleUnavailableException)
            {
                ModelState.AddModelError(nameof(CourseEditInputModel.Title), "This title already exists");
            }
            catch (OptimisticConcurrencyException)
            {
                ModelState.AddModelError("", "Sorry, the save failed because another user updated the course in the meantime. Please refresh the page and redo your changes.");
            }
        }

        ViewData["Title"] = "Edit Course";
        return View(inputModel);
    }

    [HttpPost]
    [Authorize(Policy = nameof(Policy.CourseAuthor))]
    [Authorize(Roles = nameof(Role.Teacher))]
    public async Task<IActionResult> Delete(CourseDeleteInputModel inputModel)
    {
        await courseService.DeleteCourseAsync(inputModel);
        TempData["ConfirmationMessage"] = "The course has been deleted. It may still appear in listings briefly until the cache refreshes.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = nameof(Policy.CourseSubscriber))]
    public async Task<IActionResult> Vote(int id)
    {
        CourseVoteInputModel inputModel = new()
        {
            Id = id,
            Vote = await courseService.GetCourseVoteAsync(id) ?? 0
        };

        return View(inputModel);
    }

    [Authorize(Policy = nameof(Policy.CourseSubscriber))]
    [HttpPost]
    public async Task<IActionResult> Vote(CourseVoteInputModel inputModel)
    {
        await courseService.VoteCourseAsync(inputModel);
        TempData["ConfirmationMessage"] = "Thanks for rating the course!";
        return RedirectToAction(nameof(Detail), new { id = inputModel.Id });
    }

    [Authorize(Roles = nameof(Role.Teacher))]
    public async Task<IActionResult> IsTitleAvailable(string title, int id = 0)
    {
        bool result = await courseService.IsTitleAvailableAsync(title, id);
        return Json(result);
    }
}
