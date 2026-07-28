using ContactsManager.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.Web.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    [Route("Error")]
    public ActionResult Error(int? statusCode = null, string? returnUrl = null)
    {
        var effectiveStatusCode = statusCode ?? StatusCodes.Status500InternalServerError;
        Response.StatusCode = effectiveStatusCode;

        var model = ErrorViewModel.CreateDefault(effectiveStatusCode);

        if (TempData["ResultErrorCode"] is string resultErrorCode)
        {
            model = model with { ErrorCode = resultErrorCode };
        }

        if (TempData["ResultErrorMessage"] is string resultErrorMessage)
        {
            model = model with { Message = resultErrorMessage };
        }

        return View("~/Views/Shared/Error.cshtml", model);
    }
}
