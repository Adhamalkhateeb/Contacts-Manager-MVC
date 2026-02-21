using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ContactsManager.Controllers
{
    [Route("[controller]")]
    public class PersonsController : Controller
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;

        public PersonsController(IPersonsService personsService, ICountriesService countriesService)
        {
            _personsService = personsService;
            _countriesService = countriesService;
        }
        [Route("[action]")]
        [Route("/")]
        public IActionResult Index(string searchBy, string? searchValue,
                string orderBy = nameof(PersonResponse.Name), SortOrder sortOrder = SortOrder.ASC)
        {
            ViewBag.SearchFields = new Dictionary<string, string>
            {
                {nameof(PersonResponse.Name),"Name"},
                {nameof(PersonResponse.Email),"Email"},
                {nameof(PersonResponse.DateOfBirth),"Date Of Birth"},
                {nameof(PersonResponse.Gender),"Gender"},
                {nameof(PersonResponse.Country),"Country"},
                {nameof(PersonResponse.Address),"Address"},
                {nameof(PersonResponse.ReceiveNewsLetters),"Receive News Letter"}
            };
            var filteredPersons = _personsService.GetFiltered(searchBy, searchValue);
            var sortedPersons = _personsService.GetSorted(filteredPersons, orderBy, sortOrder);

            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchValue = searchValue;
            ViewBag.CurrentOrderBy = orderBy;
            ViewBag.CurrentSortOrder = sortOrder.ToString();

            return View(sortedPersons);
        }

        [Route("[action]")]
        [HttpGet]
        public IActionResult Create()
        {
            var countries = _countriesService.GetAll();
            ViewBag.Countries = countries;
            return View();
        }

        [Route("[action]")]
        [HttpPost]
        public IActionResult Create(PersonAddRequest request)
        {
            if (!ModelState.IsValid)
            {
                var countries = _countriesService.GetAll();
                ViewBag.Countries = countries;
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage).ToList();
                return View();
            }
            var response = _personsService.Add(request);
            return RedirectToAction("Index", "Persons");
        }

    }
}
