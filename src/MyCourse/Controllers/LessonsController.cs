using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.Exceptions.Application;
using MyCourse.Models.InputModels.Lessons;
using MyCourse.Models.ViewModels.Lessons;

namespace MyCourse.Controllers;

public class LessonsController : Controller
{
    private readonly ICachedLessonService lessonService;

    public LessonsController(ICachedLessonService lessonService)
    {
        this.lessonService = lessonService;
    }

    // Note: specifying two or more policy names separated by commas is not
    // supported by ASP.NET Core out of the box. A custom policy provider was needed;
    // see Models/Authorization/MultiAuthorizationPolicyProvider.cs,
    // which is registered in the RegisterServices method of the Startup class.
    [Authorize(Policy = nameof(Policy.CourseAuthor) + "," + nameof(Policy.CourseSubscriber))]
    public async Task<IActionResult> Detail(int id)
    {
        LessonDetailViewModel viewModel = await lessonService.GetLessonAsync(id);
        ViewData["Title"] = viewModel.Title;
        return View(viewModel);
    }

    [Authorize(Roles = nameof(Role.Teacher))]
    [Authorize(Policy = nameof(Policy.CourseAuthor))]
    public IActionResult Create(int id)
    {
        ViewData["Title"] = "New Lesson";
        LessonCreateInputModel inputModel = new();
        inputModel.CourseId = id;
        return View(inputModel);
    }

    [HttpPost]
    [Authorize(Roles = nameof(Role.Teacher))]
    [Authorize(Policy = nameof(Policy.CourseAuthor))]
    public async Task<IActionResult> Create(LessonCreateInputModel inputModel)
    {
        if (ModelState.IsValid)
        {
            LessonDetailViewModel lesson = await lessonService.CreateLessonAsync(inputModel);
            TempData["ConfirmationMessage"] = "Lesson created! Go ahead and fill in the remaining details.";
            return RedirectToAction(nameof(Edit), new { id = lesson.Id });
        }

        ViewData["Title"] = "New Lesson";
        return View(inputModel);

    }

    [Authorize(Roles = nameof(Role.Teacher))]
    [Authorize(Policy = nameof(Policy.CourseAuthor))]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Lesson";
        LessonEditInputModel inputModel = await lessonService.GetLessonForEditingAsync(id);
        return View(inputModel);
    }

    [HttpPost]
    [Authorize(Roles = nameof(Role.Teacher))]
    [Authorize(Policy = nameof(Policy.CourseAuthor))]
    public async Task<IActionResult> Edit(LessonEditInputModel inputModel)
    {
        if (ModelState.IsValid)
        {
            try
            {
                LessonDetailViewModel viewModel = await lessonService.EditLessonAsync(inputModel);
                TempData["ConfirmationMessage"] = "Changes saved successfully.";
                return RedirectToAction(nameof(Detail), new { id = viewModel.Id });
            }
            catch (OptimisticConcurrencyException)
            {
                ModelState.AddModelError("", "Sorry, the save failed because another user updated this lesson in the meantime. Please refresh the page and apply your changes again.");
            }
        }

        ViewData["Title"] = "Edit Lesson";
        return View(inputModel);
    }

    [HttpPost]
    [Authorize(Roles = nameof(Role.Teacher))]
    [Authorize(Policy = nameof(Policy.CourseAuthor))]
    public async Task<IActionResult> Delete(LessonDeleteInputModel inputModel)
    {
        await lessonService.DeleteLessonAsync(inputModel);
        TempData["ConfirmationMessage"] = "The lesson has been deleted.";
        return RedirectToAction(nameof(CoursesController.Detail), "Courses", new { id = inputModel.CourseId });
    }
}
