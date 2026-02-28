
using ContactsManager.Controllers;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;


[Route("[controller]")]
public class CountriesController : Controller
{
    private readonly ICountriesService _countriesService;

    public CountriesController(ICountriesService countriesService)
    {
        _countriesService = countriesService;
    }

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

        if (!Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            ViewBag.ErrorMessage = "Unsupported file, select an (.xlsx) file.";
            return View();
        }

        try
        {
            var count = await _countriesService.UploadFromExcelFileAsync(excelFile);
            ViewBag.Message = $"{count} countries added successfully";
            return View();
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = $"{ex.Message}";
            return View();
        }
    }
}