using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using MyCourse.Models.InputModels.Courses;

namespace MyCourse.Customizations.ModelBinders;

public class CourseListInputModelBinder : IModelBinder
{
    private readonly IOptionsMonitor<CoursesOptions> coursesOptions;
    public CourseListInputModelBinder(IOptionsMonitor<CoursesOptions> coursesOptions)
    {
        this.coursesOptions = coursesOptions;
    }
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        //Retrieve values from value providers
        string search = bindingContext.ValueProvider.GetValue("Search").FirstValue;
        string orderBy = bindingContext.ValueProvider.GetValue("OrderBy").FirstValue;
        int.TryParse(bindingContext.ValueProvider.GetValue("Page").FirstValue, out int page);
        bool.TryParse(bindingContext.ValueProvider.GetValue("Ascending").FirstValue, out bool ascending);

        //Create the CourseListInputModel instance
        CoursesOptions options = coursesOptions.CurrentValue;
        CourseListInputModel inputModel = new(search, page, orderBy, ascending, options.PerPage, options.Order);

        //Set the result to notify that the binding succeeded
        bindingContext.Result = ModelBindingResult.Success(inputModel);

        //Return a completed task
        return Task.CompletedTask;
    }
}
