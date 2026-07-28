using ContactsManager.Domain.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.Web.Controllers;

public class MvcController : Controller
{
    protected IActionResult HandleError<TModel>(List<Error> errors, TModel model) =>
        HandleErrorInternal(errors, () => View(model));

    protected IActionResult HandleError(List<Error> errors) =>
        HandleErrorInternal(errors, () => View());

    private IActionResult HandleErrorInternal(List<Error> errors, Func<IActionResult> onValidation)
    {
        if (errors is null || errors.Count == 0)
        {
            return RedirectToAction(
                "Error",
                "Home",
                new { statusCode = StatusCodes.Status500InternalServerError }
            );
        }

        if (errors.All(e => e.Type == ErrorKind.Validation || e.Type == ErrorKind.Conflict))
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return onValidation();
        }

        var primaryError = errors[0];
        var statusCode = MapToStatusCode(primaryError.Type);

        SetTempData(errors, primaryError);

        return RedirectToAction("Error", "Home", new { statusCode });
    }

    private static int MapToStatusCode(ErrorKind type) =>
        type switch
        {
            ErrorKind.Validation => StatusCodes.Status400BadRequest,
            ErrorKind.Conflict => StatusCodes.Status409Conflict,
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            ErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorKind.Forbidden => StatusCodes.Status403Forbidden,
            ErrorKind.Failure => StatusCodes.Status500InternalServerError,
            ErrorKind.Unexpected => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError,
        };

    private void SetTempData(List<Error> errors, Error primaryError)
    {
        if (TempData is null)
            return;

        TempData["ResultErrorCode"] = primaryError.Code;
        TempData["ResultErrorMessage"] = primaryError.Description;
    }
}
