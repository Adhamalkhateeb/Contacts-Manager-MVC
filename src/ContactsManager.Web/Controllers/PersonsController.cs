using ContactsManager.Application.Features.Countries.Queries.GetCountries;
using ContactsManager.Application.Features.Persons.Commands.DeletePerson;
using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Application.Features.Persons.Enums;
using ContactsManager.Application.Features.Persons.GetPersonsCSV;
using ContactsManager.Application.Features.Persons.Queries;
using ContactsManager.Application.Features.Persons.Queries.GetFilteredPersons;
using ContactsManager.Application.Features.Persons.Queries.GetPersonById;
using ContactsManager.Application.Features.Persons.Queries.GetPersonsAsExcel;
using ContactsManager.Application.Features.Persons.Queries.GetSortedPersons;
using ContactsManager.Contracts.Requests.Person;
using ContactsManager.Contracts.Responses;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Web.Filters.ActionFilters;
using ContactsManager.Web.Models;
using ContactsManager.Web.Models.Mappers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace ContactsManager.Web.Controllers;

[Route("[controller]")]
public class PersonsController : MvcController
{
    private readonly IMediator _mediator;
    private readonly ILogger<PersonsController> _logger;

    public PersonsController(IMediator mediator, ILogger<PersonsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [Route("[action]")]
    [Route("/")]
    public async Task<IActionResult> Index(
        string searchBy,
        string? searchValue,
        string orderBy = nameof(PersonDto.Name),
        SortOrder sortOrder = SortOrder.ASC,
        CancellationToken cancellationToken = default
    )
    {
        var filterResult = await _mediator.Send(
            new GetFilteredPersonsQuery(searchBy, searchValue),
            cancellationToken
        );

        PersonsListViewModel viewModel = new PersonsListViewModel
        {
            SearchBy = searchBy,
            SearchValue = searchValue,
            OrderBy = orderBy,
            SortOrder = sortOrder.ToString(),
            SearchFields = GetSearchFields(),
        };

        return await filterResult.Match(
            async filtered =>
            {
                var sortResult = await _mediator.Send(
                    new GetSortedPersonsQuery(filtered, orderBy, sortOrder),
                    cancellationToken
                );

                return sortResult.Match(
                    sorted =>
                        View(
                            nameof(Index),
                            viewModel with
                            {
                                Persons = sorted.ToPersonResponses(),
                            }
                        ),
                    errors =>
                        HandleError(
                            errors,
                            viewModel with
                            {
                                Persons = filtered.ToPersonResponses(),
                            }
                        )
                );
            },
            errors =>
                Task.FromResult(
                    HandleError(errors, viewModel with { Persons = new List<PersonResponse>() })
                )
        );
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var listResult = await GetCountriesSelectListAsync(cancellationToken);
        ViewBag.Countries = listResult.Match(list => list, _ => new List<SelectListItem>());
        return View();
    }

    [HttpPost("[action]")]
    [PersonPostFilterFactory]
    public async Task<IActionResult> Create(
        CreatePersonRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(request.ToCreatePersonCommand(), cancellationToken);

        return result.Match(
            _ => RedirectToAction(nameof(Index)),
            errors => HandleError(errors, request)
        );
    }

    [HttpGet("[action]/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var personResult = await _mediator.Send(new GetPersonByIdQuery(id), cancellationToken);

        return await personResult.Match(
            async person =>
            {
                var countriesResult = await GetCountriesSelectListAsync(cancellationToken);

                return countriesResult.Match(
                    countries =>
                    {
                        ViewBag.Countries = countries;
                        return (IActionResult)View(person.ToUpdatePersonRequest());
                    },
                    errors => HandleError(errors, person.ToUpdatePersonRequest())
                );
            },
            errors => Task.FromResult((IActionResult)RedirectToAction(nameof(Index)))
        );
    }

    [HttpPost]
    [Route("[action]/{id}")]
    [PersonPostFilterFactory]
    public async Task<IActionResult> Edit(
        UpdatePersonRequest request,
        CancellationToken cancellationToken
    )
    {
        var updateResult = await _mediator.Send(request.ToUpdatePersonCommand(), cancellationToken);

        return updateResult.Match(
            _ => RedirectToAction(nameof(Index)),
            errors => HandleError(errors, request)
        );
    }

    [HttpGet]
    [Route("[action]/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPersonByIdQuery(id), cancellationToken);

        return result.Match(
            person => View(person.ToPersonResponse()),
            errors => HandleError(errors, id)
        );
    }

    [HttpPost]
    [Route("[action]/{id}")]
    public async Task<IActionResult> Delete(
        PersonResponse person,
        CancellationToken cancellationToken
    )
    {
        var removeResult = await _mediator.Send(
            new RemovePersonCommand(person.Id),
            cancellationToken
        );

        return removeResult.Match(
            _ => RedirectToAction(nameof(Index)),
            errors => HandleError(errors, person)
        );
    }

    [Route("PersonsPdf")]
    public async Task<IActionResult> PersonsPDF(CancellationToken cancellationToken)
    {
        var personsResult = await _mediator.Send(new GetPersonsQuery(), cancellationToken);

        return personsResult.Match(
            persons => new ViewAsPdf("PersonsPDF", persons.ToPersonResponses(), ViewData)
            {
                PageMargins = new Margins()
                {
                    Top = 20,
                    Bottom = 20,
                    Left = 20,
                    Right = 20,
                },
                PageOrientation = Orientation.Landscape,
            },
            errors => HandleError(errors, new List<PersonResponse>())
        );
    }

    [Route("PersonsCSV")]
    public async Task<IActionResult> DownloadPersonsCsv()
    {
        var csvResult = await _mediator.Send(new GetPersonsCsvQuery());
        return csvResult.Match(
            csvBytes => File(csvBytes, "text/csv", "persons.csv"),
            errors => HandleError(errors, new List<PersonResponse>())
        );
    }

    [Route("PersonsExcel")]
    public async Task<IActionResult> DownloadPersonsExcel()
    {
        var excelResult = await _mediator.Send(new GetPersonsAsExcelQuery());

        return excelResult.Match(
            fileBytes =>
            {
                var fileName = $"Persons_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            },
            errors => HandleError(errors, new List<PersonResponse>())
        );
    }

    private async Task<Result<List<SelectListItem>>> GetCountriesSelectListAsync(
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(new GetCountriesQuery(), cancellationToken);

        return result.Match(
            countries =>
                (Result<List<SelectListItem>>)
                    countries
                        .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() })
                        .ToList(),
            errors => errors
        );
    }

    private Dictionary<string, string> GetSearchFields() =>
        new()
        {
            { nameof(PersonResponse.Name), "Name" },
            { nameof(PersonResponse.Email), "Email" },
            { nameof(PersonResponse.DateOfBirth), "Date of Birth" },
            { nameof(PersonResponse.Gender), "Gender" },
            { nameof(PersonResponse.CountryId), "Country" },
            { nameof(PersonResponse.Address), "Address" },
        };
}
