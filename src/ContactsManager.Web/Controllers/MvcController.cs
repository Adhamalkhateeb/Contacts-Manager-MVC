using ContactsManager.Domain.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.Web.Controllers;

public class MvcController : Controller
{
    protected IActionResult HandleError<TModel>(List<Error> errors, TModel? model)
    {
        if (errors.All(e => e.Type == ErrorKind.Validation))
        {
            errors.ForEach(e => ModelState.AddModelError(e.Code, e.Description));
            return View(model);
        }

        var statusCode = errors[0].Type switch
        {
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            ErrorKind.Unauthorized => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

        return RedirectToAction("Error", "Home", new { statusCode });
    }
}
