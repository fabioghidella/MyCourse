using System.ComponentModel.DataAnnotations;
using System.Data;


namespace MyCourse.Models.InputModels.Lessons;

public class LessonEditInputModel
{
    [Required]
    public int Id { get; set; }

    public int CourseId { get; set; }

    [Required(ErrorMessage = "Title is required"),
    MinLength(5, ErrorMessage = "Title must be at least {1} characters long"),
    MaxLength(100, ErrorMessage = "Title must be no longer than {1} characters"),
    RegularExpression(@"^[0-9A-z\u00C0-\u00ff\s\.']+$", ErrorMessage = "Invalid title"), //This regular expression also includes accented characters
    Display(Name = "Title")]
    public string Title { get; set; }

    [MinLength(10, ErrorMessage = "Description must be at least {1} characters long"),
    MaxLength(4000, ErrorMessage = "Description must be no longer than {1} characters"),
    Display(Name = "Description")]
    public string Description { get; set; }

    [Display(Name = "Estimated duration"),
    Required(ErrorMessage = "Duration is required")]
    public TimeSpan Duration { get; set; }

    [Display(Name = "Order"),
    Required(ErrorMessage = "Order is required")]
    public int Order { get; set; }
    public string RowVersion { get; set; }


    public static LessonEditInputModel FromDataRow(DataRow courseRow)
    {
        LessonEditInputModel lessonEditInputModel = new()
        {
            Id = Convert.ToInt32(courseRow["Id"]),
            CourseId = Convert.ToInt32(courseRow["CourseId"]),
            Title = Convert.ToString(courseRow["Title"]),
            Description = Convert.ToString(courseRow["Description"]),
            Duration = TimeSpan.Parse(Convert.ToString(courseRow["Duration"])),
            Order = Convert.ToInt32(courseRow["Order"]),
            RowVersion = Convert.ToString(courseRow["RowVersion"])
        };
        return lessonEditInputModel;
    }

    public static LessonEditInputModel FromEntity(Lesson lesson)
    {
        return new LessonEditInputModel
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Description = lesson.Description,
            Duration = lesson.Duration,
            Order = lesson.Order,
            RowVersion = lesson.RowVersion
        };
    }
}
