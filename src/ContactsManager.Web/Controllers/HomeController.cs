using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Namespace
{
    public class HomeController : Controller
    {
        [Route("Error")]
        public ActionResult Error()
        {
            var exceptionHandler = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandler != null && exceptionHandler.Error != null)
            {
                ViewBag.ErrorMessage = exceptionHandler.Error.Message;
            }
            return View();
        }
    }
}
