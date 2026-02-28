using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MyCourse.Models.InputModels.Courses;

namespace MyCourse.Customizations.TagHelpers;

public class OrderLinkTagHelper : AnchorTagHelper
{
    public string OrderBy { get; set; }
    public CourseListInputModel Input { get; set; }

    public OrderLinkTagHelper(IHtmlGenerator generator) : base(generator)
    {
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";

        //Set the link route values
        RouteValues["search"] = Input.Search;
        RouteValues["orderby"] = OrderBy;
        RouteValues["ascending"] = (Input.OrderBy == OrderBy ? !Input.Ascending : Input.Ascending).ToString().ToLowerInvariant();

        //Delegate rendering to the AnchorTagHelper
        base.Process(context, output);

        //Add the sort direction indicator
        if (Input.OrderBy == OrderBy)
        {
            var direction = Input.Ascending ? "up" : "down";
            output.PostContent.SetHtmlContent($" <i class=\"fas fa-caret-{direction}\"></i>");
        }
    }
}
