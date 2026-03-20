using ContactsManager.Application.Features.Countries.Commands.UploadCountryFromExcel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.Web.Controllers;

[Route("[controller]")]
public class CountriesController(IMediator mediator) : MvcController
{
    [Route("uploadFromExcel")]
    [HttpGet]
    public IActionResult UploadFromExcel() => View();

    [Route("uploadFromExcel")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = 10_485_760)]
    public async Task<IActionResult> UploadFromExcel(IFormFile excelFile)
    {
        if (excelFile == null || excelFile.Length == 0)
        {
            ViewBag.ErrorMessage = "Please select an Excel(.xlsx) file.";
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
            ViewBag.ErrorMessage = "Unsupported file, select an (.xlsx) file.";
            return View();
        }

        var uploadResult = await mediator.Send(new UploadCountriesFromExcelCommand(excelFile));

        return uploadResult.Match(
            summary =>
            {
                if (summary.ParsedCount == 0)
                {
                    ViewBag.WarningMessage =
                        "No country rows were found in the file. Ensure the first column contains country names starting from row 2.";
                    return View();
                }

                if (summary.InsertedCount == 0)
                {
                    ViewBag.WarningMessage =
                        "No new countries were added. All rows were duplicates or invalid.";
                    ViewBag.Message =
                        $"Parsed: {summary.ParsedCount}, Duplicates: {summary.DuplicateCount}, Invalid: {summary.InvalidCount}.";
                    return View();
                }

                ViewBag.Message =
                    $"Upload completed. Added {summary.InsertedCount} of {summary.ParsedCount} rows (Duplicates: {summary.DuplicateCount}, Invalid: {summary.InvalidCount}).";
                return View();
            },
            errors =>
            {
                var primary = errors[0];
                ViewBag.ErrorMessage = $"{primary.Description} (Code: {primary.Code})";
                ViewBag.ErrorDetails = string.Join(
                    " | ",
                    errors.Select(e => $"{e.Code}: {e.Description}").Distinct()
                );
                return View();
            }
        );
    }
}
