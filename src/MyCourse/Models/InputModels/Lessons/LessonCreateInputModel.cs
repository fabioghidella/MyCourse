using System.ComponentModel.DataAnnotations;

namespace MyCourse.Models.InputModels.Lessons;

public class LessonCreateInputModel
{
    [Required]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Title is required"),
    MinLength(5, ErrorMessage = "Title must be at least {1} characters long"),
    MaxLength(100, ErrorMessage = "Title must be no longer than {1} characters"),
    RegularExpression(@"^[0-9A-z\u00C0-\u00ff\s\.']+$", ErrorMessage = "Invalid title")] //This regular expression also includes accented characters
    public string Title { get; set; }
}
