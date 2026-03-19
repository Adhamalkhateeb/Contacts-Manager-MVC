using ContactsManager.Application.Features.Countries.Commands.UploadCountryFromExcel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
public class CountriesController(IMediator mediator) : Controller
{
    [Route("uploadFromExcel")]
    [HttpGet]
    public IActionResult UploadFromExcel() => View();

    [Route("uploadFromExcel")]
    [HttpPost]
    public async Task<IActionResult> UploadFromExcel(IFormFile excelFile)
    {
        if (excelFile == null || excelFile.Length == 0)
        {
            ViewBag.ErrorMessage = "Please select an Excel(.xlsx) file.";
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
            count =>
            {
                ViewBag.Message = $"{count} countries added successfully";
                return View();
            },
            errors =>
            {
                ViewBag.ErrorMessage = errors[0].Description;
                return View();
            }
        );
    }
}
