using System.Net.NetworkInformation;
using ContactsManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ContactsManager.Controllers
{
    [Route("[controller]")]
    public class PersonsController(IPersonsService personsService, ICountriesService countriesService) : Controller
    {
        private readonly IPersonsService _personsService = personsService;
        private readonly ICountriesService _countriesService = countriesService;

        [Route("[action]")]
        [Route("/")]
        [Route("")]
        [HttpGet]
        public async Task<IActionResult> Index(
            string searchBy,
            string? searchValue,
            string orderBy = nameof(PersonResponse.Name),
            SortOrder sortOrder = SortOrder.ASC)
        {

            var filtered = await _personsService.GetFilteredAsync(searchBy, searchValue);
            var sorted = _personsService.GetSorted(filtered, orderBy, sortOrder);

            var viewModel = new PersonsListViewModel
            {
                Persons = sorted,
                SearchBy = searchBy,
                SearchValue = searchValue,
                OrderBy = orderBy,
                SortOrder = sortOrder.ToString(),
                SearchFields = GetSearchFields()
            };


            return View(viewModel);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Countries = await GetCountriesSelectListAsync();
            return View();
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> Create(PersonAddRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Countries = await GetCountriesSelectListAsync();
                PopulateErrors();
                return View(request);
            }
            await _personsService.AddAsync(request);
            return RedirectToAction(nameof(Index));
        }



        [HttpGet]
        [Route("[action]/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id)
        {

            var person = await _personsService.GetByIdAsync(id);

            if (person is null)
                return RedirectToAction(nameof(Index));

            ViewBag.Countries = await GetCountriesSelectListAsync();

            return View(person.ToPersonUpdateRequest());
        }


        [HttpPost]
        [Route("[action]/{id}")]
        public async Task<IActionResult> Edit(PersonUpdateRequest personUpdateRequest)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Countries = await GetCountriesSelectListAsync();
                PopulateErrors();
                return View(personUpdateRequest);
            }

            var personResponse = await _personsService.GetByIdAsync(personUpdateRequest.Id);

            if (personResponse is null)
                return RedirectToAction(nameof(Index));


            await _personsService.UpdateAsync(personUpdateRequest);

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        [Route("[action]/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var person = await _personsService.GetByIdAsync(id);

            if (person is null)
                return RedirectToAction(nameof(Index));

            return View(person);
        }


        [HttpPost]
        [Route("[action]/{id}")]
        public async Task<IActionResult> Delete(PersonResponse person)
        {
            if (!ModelState.IsValid)
            {
                return View(person);
            }

            var personResponse = await _personsService.GetByIdAsync(person.Id);

            if (personResponse is null)
                return RedirectToAction(nameof(Index));

            await _personsService.DeleteAsync(personResponse.Id);

            return RedirectToAction(nameof(Index));
        }

        [Route("PersonsPdf")]
        public async Task<IActionResult> PersonsPDF()
        {
            var persons = await _personsService.GetAllAsync();
            return new ViewAsPdf("PersonsPDF", persons, ViewData)
            {
                PageMargins = new Margins() { Top = 20, Bottom = 20, Left = 20, Right = 20 },
                PageOrientation = Orientation.Landscape
            };
        }


        [Route("PersonsCSV")]
        public async Task<IActionResult> DownloadPersonsCsv()
        {
            var csvBytes = await _personsService.GetPersonsCsvAsync();
            return File(csvBytes, "text/csv", "persons.csv");
        }

        [Route("PersonsExcel")]
        public async Task<IActionResult> DownloadPersonsExcel()
        {
            var fileBytes = await _personsService.GetPersonsExcelAsync();

            var fileName = $"Persons_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

        private async Task<List<SelectListItem>> GetCountriesSelectListAsync()
        {
            var countries = await _countriesService.GetAllAsync();

            return countries
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
                .ToList();
        }

        private void PopulateErrors()
        {
            ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage).ToList();
        }

        private Dictionary<string, string> GetSearchFields()
        {
            return new Dictionary<string, string>
            {
                { nameof(PersonResponse.Name), "Name" },
                { nameof(PersonResponse.Email), "Email" },
                { nameof(PersonResponse.DateOfBirth), "Date of Birth" },
                { nameof(PersonResponse.Gender), "Gender" },
                { nameof(PersonResponse.CountryId), "Country" },
                { nameof(PersonResponse.Address), "Address" },
            };
        }
    }
}
