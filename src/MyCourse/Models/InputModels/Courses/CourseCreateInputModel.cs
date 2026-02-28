using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Controllers;

namespace MyCourse.Models.InputModels.Courses;

public class CourseCreateInputModel
{
    [Required(ErrorMessage = "Title is required"),
    MinLength(10, ErrorMessage = "Title must be at least {1} characters long"),
    MaxLength(100, ErrorMessage = "Title must be no longer than {1} characters"),
    RegularExpression(@"^[0-9A-z\u00C0-\u00ff\s\.']+$", ErrorMessage = "Invalid title"), //This regular expression also includes accented characters
    Remote(action: nameof(CoursesController.IsTitleAvailable), controller: "Courses", ErrorMessage = "This title already exists")]
    public string Title { get; set; }
}
