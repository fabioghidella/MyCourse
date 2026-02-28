using System.ComponentModel.DataAnnotations;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyCourse.Models.ViewModels.Courses;

namespace MyCourse.Pages;

[ValidateReCaptcha]
public class ContactModel : PageModel
{
    public CourseDetailViewModel Course { get; private set; }

    [Required(ErrorMessage = "The question text is required")]
    [Display(Name = "Your question")]
    [BindProperty]
    public string Question { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, [FromServices] ICourseService courseService)
    {
        try
        {
            Course = await courseService.GetCourseAsync(id);
            ViewData["Title"] = $"Send a Question";
            return Page();
        }
        catch
        {
            return RedirectToAction("Index", "Courses");
        }
    }

    public async Task<IActionResult> OnPostAsync(int id, [FromServices] ICourseService courseService)
    {
        if (ModelState.IsValid)
        {
            await courseService.SendQuestionToCourseAuthorAsync(id, Question);
            TempData["ConfirmationMessage"] = "Your question has been sent";
            return RedirectToAction("Detail", "Courses", new { id = id });
        }
        else
        {
            return await OnGetAsync(id, courseService);
        }
    }
}
