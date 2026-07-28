using ContactsManager.Application.Features.Countries.Commands.UploadCountryFromExcel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.Web.Controllers;

[Route("[controller]")]
[Authorize(Policy = "AdminOnly")]
public class CountriesController(IMediator mediator) : MvcController
{
    [Route("uploadFromExcel")]
    [HttpGet]
    public IActionResult UploadFromExcel() => View();

    [Route("uploadFromExcel")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadFromExcel(IFormFile excelFile)
    {
        if (excelFile is null || excelFile.Length == 0)
        {
            ViewBag.ErrorMessage = "Please select an Excel (.xlsx) file.";
            return View();
        }

        if (excelFile.Length > 10_485_760)
        {
            ViewBag.ErrorMessage = "File size exceeds 10 MB.";
            return View();
        }

        if (
            !Path.GetExtension(excelFile.FileName)
                .Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
        )
        {
            ViewBag.ErrorMessage = "Unsupported file — please select an .xlsx file.";
            return View();
        }

        await using var stream = excelFile.OpenReadStream();

        var command = new UploadCountriesFromExcelCommand(
            stream,
            excelFile.FileName,
            excelFile.Length
        );

        var uploadResult = await mediator.Send(command);

        return uploadResult.Match(
            summary =>
            {
                if (summary.ParsedCount == 0)
                {
                    ViewBag.WarningMessage =
                        "No country rows found. Ensure the first column contains country names starting from row 2.";
                    return View();
                }

                if (summary.InsertedCount == 0)
                {
                    ViewBag.WarningMessage =
                        "No new countries were added — all rows were duplicates or invalid.";
                    ViewBag.Message =
                        $"Parsed: {summary.ParsedCount} | Duplicates: {summary.DuplicateCount} | Invalid: {summary.InvalidCount}";
                    return View();
                }

                ViewBag.Message =
                    $"Upload complete. Added {summary.InsertedCount} of {summary.ParsedCount} "
                    + $"(Duplicates: {summary.DuplicateCount}, Invalid: {summary.InvalidCount}).";
                return View();
            },
            errors =>
            {
                var primary = errors[0];
                ViewBag.ErrorMessage = $"{primary.Description} (Code: {primary.Code})";

                if (errors.Count > 1)
                {
                    ViewBag.ErrorDetails = string.Join(
                        " | ",
                        errors.Select(e => $"{e.Code}: {e.Description}").Distinct()
                    );
                }

                return View();
            }
        );
    }
}
