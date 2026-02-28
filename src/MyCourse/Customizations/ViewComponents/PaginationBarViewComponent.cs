using Microsoft.AspNetCore.Mvc;

namespace MyCourse.Customizations.ViewComponents;

public class PaginationBarViewComponent : ViewComponent
{
    //public IViewComponentResult Invoke(CourseListViewModel model)
    public IViewComponentResult Invoke(IPaginationInfo model)
    {
        //Current page number
        //Total number of results
        //Results per page
        //Search, OrderBy and Ascending
        return View(model);
    }
}
