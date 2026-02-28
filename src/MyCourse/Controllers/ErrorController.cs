using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyCourse.Models.Exceptions.Application;
using MyCourse.Models.Exceptions.Infrastructure;

namespace MyCourse.Controllers;

public class ErrorController : Controller
{
    [AllowAnonymous]
    public IActionResult Index()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        switch (feature.Error)
        {
            case CourseNotFoundException exc:
                ViewData["Title"] = "Course Not Found";
                Response.StatusCode = 404;
                return View("CourseNotFound");

            case CourseSubscriptionException exc:
                ViewData["Title"] = "Could not complete your subscription";
                Response.StatusCode = 400;
                return View();

            case CourseSubscriptionNotFoundException exc:
                ViewData["Title"] = "You are not subscribed to this course";
                Response.StatusCode = 400;
                return View();

            case PaymentGatewayException exc:
                ViewData["Title"] = "A payment error occurred";
                Response.StatusCode = 400;
                return View();

            case UserUnknownException exc:
                ViewData["Title"] = "Unknown user";
                Response.StatusCode = 400;
                return View();

            case SendException exc:
                ViewData["Title"] = "Message could not be sent, please try again later";
                Response.StatusCode = 500;
                return View();

            default:
                ViewData["Title"] = "Error";
                return View();
        }
    }
}
