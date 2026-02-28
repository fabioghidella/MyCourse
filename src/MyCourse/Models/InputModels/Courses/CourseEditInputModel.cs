using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Controllers;

namespace MyCourse.Models.InputModels.Courses;

public class CourseEditInputModel : IValidatableObject
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required"),
    MinLength(10, ErrorMessage = "Title must be at least {1} characters long"),
    MaxLength(100, ErrorMessage = "Title must be no longer than {1} characters"),
    RegularExpression(@"^[0-9A-z\u00C0-\u00ff\s\.']+$", ErrorMessage = "Invalid title"), //This regular expression also includes accented characters
    Remote(action: nameof(CoursesController.IsTitleAvailable), controller: "Courses", ErrorMessage = "This title already exists", AdditionalFields = "Id"),
    Display(Name = "Title")]
    public string Title { get; set; }

    [MinLength(10, ErrorMessage = "Description must be at least {1} characters long"),
    MaxLength(4000, ErrorMessage = "Description must be no longer than {1} characters"),
    Display(Name = "Description")]
    public string Description { get; set; }

    [Display(Name = "Cover image")]
    public string ImagePath { get; set; }

    [Required(ErrorMessage = "Contact email is required"),
    EmailAddress(ErrorMessage = "You must provide a valid email address"),
    Display(Name = "Contact email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Full price is required"),
    Display(Name = "Full price")]
    public Money FullPrice { get; set; }

    [Required(ErrorMessage = "Current price is required"),
    Display(Name = "Current price")]
    public Money CurrentPrice { get; set; }

    [Display(Name = "Upload new image...")]
    public IFormFile Image { get; set; }
    public string RowVersion { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FullPrice.Currency != CurrentPrice.Currency)
        {
            yield return new ValidationResult("Full price must have the same currency as the current price", new[] { nameof(FullPrice) });
        }
        else if (FullPrice.Amount < CurrentPrice.Amount)
        {
            yield return new ValidationResult("Full price cannot be less than the current price", new[] { nameof(FullPrice) });
        }
    }

    public static CourseEditInputModel FromDataRow(DataRow courseRow)
    {
        CourseEditInputModel courseEditInputModel = new()
        {
            Title = Convert.ToString(courseRow["Title"]),
            Description = Convert.ToString(courseRow["Description"]),
            ImagePath = Convert.ToString(courseRow["ImagePath"]),
            Email = Convert.ToString(courseRow["Email"]),
            FullPrice = new Money(
                Enum.Parse<Currency>(Convert.ToString(courseRow["FullPrice_Currency"])),
                Convert.ToDecimal(courseRow["FullPrice_Amount"])
            ),
            CurrentPrice = new Money(
                Enum.Parse<Currency>(Convert.ToString(courseRow["CurrentPrice_Currency"])),
                Convert.ToDecimal(courseRow["CurrentPrice_Amount"])
            ),
            Id = Convert.ToInt32(courseRow["Id"]),
            RowVersion = Convert.ToString(courseRow["RowVersion"])
        };
        return courseEditInputModel;
    }

    public static CourseEditInputModel FromEntity(Course course)
    {
        return new CourseEditInputModel
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Email = course.Email,
            ImagePath = course.ImagePath,
            CurrentPrice = course.CurrentPrice,
            FullPrice = course.FullPrice,
            RowVersion = course.RowVersion
        };
    }
}
