using System.ComponentModel.DataAnnotations;

namespace MyCourse.Models.InputModels.Users;

public class UserRoleInputModel
{
    [Required(ErrorMessage = "Email address is required"),
    EmailAddress(ErrorMessage = "The email address entered is not valid"),
    Display(Name = "Email address")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Role is required"),
    Display(Name = "Role")]
    public Role Role { get; set; }
}
