using AutoFixture;
using ContactsManager.Application.Features.Countries.DTOs;
using ContactsManager.Application.Features.Countries.Queries.GetCountries;
using ContactsManager.Application.Features.Persons.Commands.CreatePerson;
using ContactsManager.Application.Features.Persons.Commands.RemovePerson;
using ContactsManager.Application.Features.Persons.Commands.UpdatePerson;
using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Application.Features.Persons.Enums;
using ContactsManager.Application.Features.Persons.GetPersonsCSV;
using ContactsManager.Application.Features.Persons.Queries;
using ContactsManager.Application.Features.Persons.Queries.GetFilteredAndSortedPersons;
using ContactsManager.Application.Features.Persons.Queries.GetPersonsAsExcel;
using ContactsManager.Contracts.Requests.Person;
using ContactsManager.Contracts.Requests.Person.Enums;
using ContactsManager.Contracts.Responses;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Web.Controllers;
using ContactsManager.Web.Models;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Moq;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace ContactsManager.UnitTests.Persons.Controllers;

public class PersonsControllerTests
{
    private readonly PersonsController _sut;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<PersonsController>> _loggerMock;
    private readonly IFixture _fixture;

    public PersonsControllerTests()
    {
        _fixture = new Fixture();
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<PersonsController>>();

        _sut = new PersonsController(_mediatorMock.Object, _loggerMock.Object);
    }

    private CreatePersonRequest CreateValidCreateRequest() =>
        _fixture
            .Build<CreatePersonRequest>()
            .With(x => x.Name, "John Doe")
            .With(x => x.Email, "john@test.com")
            .With(x => x.Gender, Gender.Male)
            .With(x => x.CountryId, _fixture.Create<Guid>())
            .With(x => x.Address, "Address")
            .With(x => x.ReceiveNewsLetters, true)
            .With(x => x.DateOfBirth, new DateTime(1990, 1, 1))
            .Create();

    private UpdatePersonRequest CreateValidUpdateRequest(Guid id, Guid countryId) =>
        _fixture
            .Build<UpdatePersonRequest>()
            .With(x => x.Id, id)
            .With(x => x.Name, "John Updated")
            .With(x => x.Email, "john.updated@test.com")
            .With(x => x.Gender, Gender.Male)
            .With(x => x.CountryId, countryId)
            .With(x => x.Address, "Updated")
            .With(x => x.ReceiveNewsLetters, false)
            .With(x => x.DateOfBirth, new DateTime(1991, 1, 1))
            .Create();

    private PersonDto CreatePersonDto(Guid id, Guid countryId) =>
        _fixture
            .Build<PersonDto>()
            .With(x => x.Id, id)
            .With(x => x.Name, "John")
            .With(x => x.Email, "john@test.com")
            .With(x => x.CountryId, countryId)
            .With(x => x.Gender, ContactsManager.Domain.Persons.Enums.Gender.Male)
            .With(x => x.Address, "Address")
            .With(x => x.ReceiveNewsLetters, true)
            .With(x => x.DateOfBirth, new DateTime(1990, 1, 1))
            .Create();

    private static List<Error> ValidationErrors() =>
        [Error.Validation("Validation_Name", "Name is invalid")];

    private static List<Error> NotFoundErrors() =>
        [Error.NotFound("NotFound_Person", "Person not found")];

    [Fact]
    public async Task Index_WhenQueryIsValid_ThenReturnsViewModel()
    {
        var persons = _fixture.Build<PersonDto>().CreateMany(2).ToList();

        _mediatorMock
            .Setup(x =>
                x.Send(It.IsAny<GetFilteredAndSortedPersonsQuery>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(persons);

        var result = await _sut.Index(
            nameof(PersonDto.Name),
            "a",
            nameof(PersonDto.Name),
            SortOrder.ASC
        );

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<PersonsListViewModel>().Subject;
        model.Persons.Should().HaveCount(2);
    }

    [Fact]
    public async Task Index_WhenValidationError_ReturnsViewWithModelStateErrors()
    {
        _mediatorMock
            .Setup(x =>
                x.Send(It.IsAny<GetFilteredAndSortedPersonsQuery>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(ValidationErrors());

        var result = await _sut.Index(
            nameof(PersonDto.Name),
            "bad",
            nameof(PersonDto.Name),
            SortOrder.ASC
        );

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<PersonsListViewModel>();
        _sut.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Create_Get_WhenCountriesAvailable_PopulatesViewBag()
    {
        var countries = _fixture.Build<CountryDto>().CreateMany(2).ToList();
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCountriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(countries);

        var result = await _sut.Create(CancellationToken.None);

        result.Should().BeOfType<ViewResult>();
        var selectItems = _sut.ViewBag.Countries as List<SelectListItem>;
        selectItems.Should().NotBeNull();
        selectItems!.Should().HaveCount(2);
    }

    [Fact]
    public async Task Create_Get_WhenCountriesQueryFails_ReturnsView()
    {
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCountriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NotFoundErrors());

        var result = await _sut.Create(CancellationToken.None);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_Post_WhenCommandIsValid_ThenRedirectsToIndex()
    {
        var request = CreateValidCreateRequest();

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreatePersonCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Created);

        var result = await _sut.Create(request, CancellationToken.None);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(PersonsController.Index));
    }

    [Fact]
    public async Task Create_Post_WhenValidationError_ReturnsViewAndAddsModelState()
    {
        var request = CreateValidCreateRequest();

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreatePersonCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationErrors());

        var result = await _sut.Create(request, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(request);
        _sut.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_Post_WhenPersonIsValid_ThenRedirectsToIndex()
    {
        var person = _fixture
            .Build<PersonResponse>()
            .With(x => x.Id, _fixture.Create<Guid>())
            .With(x => x.Name, "John")
            .With(x => x.Email, "john@test.com")
            .With(x => x.Gender, "Male")
            .Create();

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<RemovePersonCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Deleted);

        var result = await _sut.Delete(person, CancellationToken.None);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(PersonsController.Index));
    }

    [Fact]
    public async Task Delete_Get_WhenPersonNotFound_RedirectsToError()
    {
        _mediatorMock
            .Setup(x =>
                x.Send(
                    It.IsAny<ContactsManager.Application.Features.Persons.Queries.GetPersonById.GetPersonByIdQuery>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(NotFoundErrors());

        var result = await _sut.Delete(Guid.NewGuid(), CancellationToken.None);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Error");
        redirectResult.ControllerName.Should().Be("Home");
    }

    [Fact]
    public async Task Delete_Post_WhenRemoveFails_RedirectsToError()
    {
        var person = _fixture
            .Build<PersonResponse>()
            .With(x => x.Id, _fixture.Create<Guid>())
            .With(x => x.Name, "John")
            .With(x => x.Email, "john@test.com")
            .With(x => x.Gender, "Male")
            .Create();

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<RemovePersonCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NotFoundErrors());

        var result = await _sut.Delete(person, CancellationToken.None);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Error");
        redirectResult.ControllerName.Should().Be("Home");
    }

    [Fact]
    public async Task Delete_Get_WhenPersonFound_ReturnsViewWithPersonModel()
    {
        var personId = Guid.NewGuid();
        var person = CreatePersonDto(personId, Guid.NewGuid());

        _mediatorMock
            .Setup(x =>
                x.Send(
                    It.IsAny<ContactsManager.Application.Features.Persons.Queries.GetPersonById.GetPersonByIdQuery>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(person);

        var result = await _sut.Delete(personId, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<PersonResponse>().Subject;
        model.Id.Should().Be(personId);
    }

    [Fact]
    public async Task Edit_Get_WhenIdIsValid_ThenReturnsView()
    {
        var personId = _fixture.Create<Guid>();
        var person = _fixture
            .Build<PersonDto>()
            .With(x => x.Id, personId)
            .With(x => x.Name, "John")
            .With(x => x.Email, "john@test.com")
            .With(x => x.CountryId, _fixture.Create<Guid>())
            .With(x => x.Gender, ContactsManager.Domain.Persons.Enums.Gender.Male)
            .Create();

        var countries = _fixture
            .Build<CountryDto>()
            .With(x => x.Id, _fixture.Create<Guid>())
            .With(x => x.Name, "Egypt")
            .CreateMany(1)
            .ToList();

        _mediatorMock
            .Setup(x =>
                x.Send(
                    It.IsAny<ContactsManager.Application.Features.Persons.Queries.GetPersonById.GetPersonByIdQuery>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(person);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCountriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(countries);

        var result = await _sut.Edit(personId, CancellationToken.None);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Edit_Get_WhenPersonNotFound_RedirectsToError()
    {
        _mediatorMock
            .Setup(x =>
                x.Send(
                    It.IsAny<ContactsManager.Application.Features.Persons.Queries.GetPersonById.GetPersonByIdQuery>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(NotFoundErrors());

        var result = await _sut.Edit(Guid.NewGuid(), CancellationToken.None);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ControllerName.Should().Be("Home");
        redirectResult.ActionName.Should().Be(nameof(HomeController.Error));
    }

    [Fact]
    public async Task Edit_Post_WhenCommandSucceeds_RedirectsToIndex()
    {
        var personId = Guid.NewGuid();
        var request = CreateValidUpdateRequest(personId, Guid.NewGuid());

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdatePersonCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Updated);

        var result = await _sut.Edit(request, CancellationToken.None);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(PersonsController.Index));
    }

    [Fact]
    public async Task Edit_Post_WhenValidationError_ReturnsViewWithModelState()
    {
        var personId = Guid.NewGuid();
        var request = CreateValidUpdateRequest(personId, Guid.NewGuid());

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdatePersonCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidationErrors());

        var result = await _sut.Edit(request, CancellationToken.None);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(request);
        _sut.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Edit_Get_WhenCountriesQueryFails_RedirectsToError()
    {
        var person = CreatePersonDto(Guid.NewGuid(), Guid.NewGuid());

        _mediatorMock
            .Setup(x =>
                x.Send(
                    It.IsAny<ContactsManager.Application.Features.Persons.Queries.GetPersonById.GetPersonByIdQuery>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(person);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCountriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NotFoundErrors());

        var result = await _sut.Edit(person.Id, CancellationToken.None);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ControllerName.Should().Be("Home");
        redirectResult.ActionName.Should().Be(nameof(HomeController.Error));
    }

    [Fact]
    public async Task PersonsPDF_WhenQuerySucceeds_ReturnsViewAsPdf()
    {
        var persons = _fixture.Build<PersonDto>().CreateMany(2).ToList();

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetPersonsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(persons);

        var result = await _sut.PersonsPDF(CancellationToken.None);

        var pdfResult = result.Should().BeOfType<ViewAsPdf>().Subject;
        pdfResult.ViewName.Should().Be("PersonsPDF");
        pdfResult.PageOrientation.Should().Be(Orientation.Landscape);
    }

    [Fact]
    public async Task PersonsPDF_WhenQueryFails_RedirectsToError()
    {
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetPersonsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NotFoundErrors());

        var result = await _sut.PersonsPDF(CancellationToken.None);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ControllerName.Should().Be("Home");
        redirectResult.ActionName.Should().Be(nameof(HomeController.Error));
    }

    [Fact]
    public async Task DownloadPersonsCsv_WhenQuerySucceeds_ReturnsCsvFile()
    {
        var csvBytes = _fixture.CreateMany<byte>(3).ToArray();

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetPersonsCsvQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(csvBytes);

        var result = await _sut.DownloadPersonsCsv();

        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("text/csv");
        fileResult.FileDownloadName.Should().Be("persons.csv");
        fileResult.FileContents.Should().Equal(csvBytes);
    }

    [Fact]
    public async Task DownloadPersonsCsv_WhenQueryFails_RedirectsToError()
    {
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetPersonsCsvQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NotFoundErrors());

        var result = await _sut.DownloadPersonsCsv();

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ControllerName.Should().Be("Home");
        redirectResult.ActionName.Should().Be(nameof(HomeController.Error));
    }

    [Fact]
    public async Task DownloadPersonsExcel_WhenQuerySucceeds_ReturnsExcelFile()
    {
        var excelBytes = _fixture.CreateMany<byte>(3).ToArray();

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetPersonsExcelQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(excelBytes);

        var result = await _sut.DownloadPersonsExcel();

        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult
            .ContentType.Should()
            .Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        fileResult.FileDownloadName.Should().StartWith("Persons_");
        fileResult.FileDownloadName.Should().EndWith(".xlsx");
        fileResult.FileContents.Should().Equal(excelBytes);
    }

    [Fact]
    public async Task DownloadPersonsExcel_WhenQueryFails_RedirectsToError()
    {
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetPersonsExcelQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NotFoundErrors());

        var result = await _sut.DownloadPersonsExcel();

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ControllerName.Should().Be("Home");
        redirectResult.ActionName.Should().Be(nameof(HomeController.Error));
    }
}
