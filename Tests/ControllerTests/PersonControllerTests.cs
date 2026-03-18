using AutoFixture;
using ContactsManager.Controllers;
using ContactsManager.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ControllerTests;

public class PersonsControllerTests
{
    private readonly PersonsController _sut;
    private readonly Mock<ICountryQueryService> _countriesServiceMock;
    private readonly Mock<IPersonQueryService> _personQueryServiceMock;
    private readonly Mock<IPersonCommandService> _personCommandServiceMock;
    private readonly Mock<ILogger<PersonsController>> _loggerMock;
    private readonly IFixture _fixture;

    public PersonsControllerTests()
    {
        _fixture = new Fixture();
        _countriesServiceMock = new Mock<ICountryQueryService>();
        _personQueryServiceMock = new Mock<IPersonQueryService>();
        _personCommandServiceMock = new Mock<IPersonCommandService>();
        _loggerMock = new Mock<ILogger<PersonsController>>();

        _sut = new PersonsController(
            _personQueryServiceMock.Object,
            _personCommandServiceMock.Object,
            _countriesServiceMock.Object,
            _loggerMock.Object
        );
    }

    #region Helpers

    private List<PersonResponse> CreatePersonsList()
    {
        return _fixture
            .Build<PersonResponse>()
            .With(p => p.Name, "Adham")
            .With(p => p.Email, "adham@test.com")
            .CreateMany(3)
            .ToList();
    }

    private List<CountryResponse> CreateCountriesList()
    {
        return _fixture.Build<CountryResponse>().CreateMany(2).ToList();
    }

    #endregion


    #region Index

    [Fact]
    public async Task Index_ShouldReturnViewWithFilteredAndSortedPersons()
    {
        // Arrange
        var persons = CreatePersonsList();
        var searchBy = nameof(PersonResponse.Name);
        var searchValue = "Adham";
        var orderBy = nameof(PersonResponse.Name);
        var sortOrder = SortOrder.ASC;

        _personQueryServiceMock
            .Setup(x => x.GetFilteredAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(persons);

        _personQueryServiceMock
            .Setup(x =>
                x.GetSorted(
                    It.IsAny<List<PersonResponse>>(),
                    It.IsAny<string>(),
                    It.IsAny<SortOrder>()
                )
            )
            .Returns(persons);

        // Act
        var result = await _sut.Index(searchBy, searchValue, orderBy, sortOrder);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<PersonsListViewModel>().Subject;

        model.Persons.Should().BeEquivalentTo(persons);
        model.SearchBy.Should().Be(searchBy);
        model.SearchValue.Should().Be(searchValue);
        model.OrderBy.Should().Be(orderBy);
        model.SortOrder.Should().Be(sortOrder.ToString());
        model.SearchFields.Should().NotBeEmpty();

        _personQueryServiceMock.Verify(x => x.GetFilteredAsync(searchBy, searchValue), Times.Once);
        _personQueryServiceMock.Verify(x => x.GetSorted(persons, orderBy, sortOrder), Times.Once);
    }

    [Fact]
    public async Task Index_WithDefaultValues_ShouldUseDefaults()
    {
        // Arrange
        var persons = CreatePersonsList();

        _personQueryServiceMock
            .Setup(x => x.GetFilteredAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(persons);

        _personQueryServiceMock
            .Setup(x =>
                x.GetSorted(
                    It.IsAny<List<PersonResponse>>(),
                    nameof(PersonResponse.Name),
                    SortOrder.ASC
                )
            )
            .Returns(persons);

        // Act
        var result = await _sut.Index(string.Empty, string.Empty);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<PersonsListViewModel>().Subject;

        model.SearchBy.Should().Be(string.Empty);
        model.SearchValue.Should().Be(string.Empty);
        model.OrderBy.Should().Be(nameof(PersonResponse.Name));
        model.SortOrder.Should().Be(SortOrder.ASC.ToString());
    }
    #endregion

    #region  Create

    [Fact]
    public async Task Create_Get_ShouldReturnViewWithCountries()
    {
        var countries = CreateCountriesList();

        _countriesServiceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(countries);

        var result = await _sut.Create();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeNull();

        var viewBagCountries = _sut.ViewBag.Countries as List<SelectListItem>;
        viewBagCountries.Should().NotBeNull();
        viewBagCountries.Should().HaveCount(countries.Count);

        _countriesServiceMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Create_Post_InvalidModel_WhenCalledDirectly_ShouldRedirectAndInvokeAdd()
    {
        var request = _fixture.Create<PersonAddRequest>();
        _sut.ModelState.AddModelError(nameof(PersonAddRequest.Name), "Person Name can't be Blank");

        var result = await _sut.Create(request);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(PersonsController.Index));

        _personCommandServiceMock.Verify(x => x.AddAsync(It.IsAny<PersonAddRequest>()), Times.Once);
    }

    [Fact]
    public async Task Create_Post_ValidModel_ShouldAddPersonAndRedirect()
    {
        var request = _fixture.Create<PersonAddRequest>();
        var response = _fixture.Create<PersonResponse>();

        _personCommandServiceMock.Setup(x => x.AddAsync(request)).ReturnsAsync(response);

        var result = await _sut.Create(request);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(PersonsController.Index));

        _personCommandServiceMock.Verify(x => x.AddAsync(request), Times.Once);
    }
    #endregion

    #region  Edit

    [Fact]
    public async Task Edit_Get_InvalidId_ShouldRedirectToIndex()
    {
        var personId = Guid.NewGuid();

        _personQueryServiceMock
            .Setup(x => x.GetByIdAsync(personId))
            .ReturnsAsync((PersonResponse?)null);

        var result = await _sut.Edit(personId);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(PersonsController.Index));
    }

    [Fact]
    public async Task Edit_Get_ValidId_ShouldReturnViewWithPerson()
    {
        var personId = Guid.NewGuid();
        var person = _fixture
            .Build<PersonResponse>()
            .With(p => p.Id, personId)
            .With(p => p.Gender, Gender.Male.ToString())
            .Create();

        var countries = CreateCountriesList();
        _countriesServiceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(countries);

        _personQueryServiceMock.Setup(x => x.GetByIdAsync(personId)).ReturnsAsync(person);

        var result = await _sut.Edit(personId);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<PersonUpdateRequest>().Subject;

        model.Id.Should().Be(personId);
        var viewBagCountries = _sut.ViewBag.Countries as List<SelectListItem>;

        viewBagCountries.Should().NotBeNull();
        viewBagCountries.Should().HaveCount(countries.Count);

        _personQueryServiceMock.Verify(x => x.GetByIdAsync(personId), Times.Once);
        _countriesServiceMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Edit_Post_InvalidModel_WhenCalledDirectly_ShouldRedirect()
    {
        var request = _fixture.Create<PersonUpdateRequest>();
        _sut.ModelState.AddModelError(
            nameof(PersonUpdateRequest.Name),
            "Person Name can't be Blank"
        );

        var result = await _sut.Edit(request);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(PersonsController.Index));

        _personCommandServiceMock.Verify(
            x => x.UpdateAsync(It.IsAny<PersonUpdateRequest>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Edit_Post_ValidModelButPersonNotFound_ShouldRedirectToIndex()
    {
        // Arrange
        var request = _fixture.Create<PersonUpdateRequest>();

        _personQueryServiceMock
            .Setup(x => x.GetByIdAsync(request.Id))
            .ReturnsAsync((PersonResponse?)null);

        // Act
        var result = await _sut.Edit(request);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(PersonsController.Index));

        _personCommandServiceMock.Verify(
            x => x.UpdateAsync(It.IsAny<PersonUpdateRequest>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Edit_Post_ValidModel_ShouldUpdateAndRedirect()
    {
        // Arrange
        var request = _fixture.Create<PersonUpdateRequest>();
        var existingPerson = _fixture.Build<PersonResponse>().With(p => p.Id, request.Id).Create();

        _personQueryServiceMock.Setup(x => x.GetByIdAsync(request.Id)).ReturnsAsync(existingPerson);

        // Act
        var result = await _sut.Edit(request);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(PersonsController.Index));

        _personCommandServiceMock.Verify(x => x.UpdateAsync(request), Times.Once);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_Get_WithValidId_ShouldReturnViewWithPerson()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var person = _fixture.Build<PersonResponse>().With(p => p.Id, personId).Create();

        _personQueryServiceMock.Setup(x => x.GetByIdAsync(personId)).ReturnsAsync(person);

        // Act
        var result = await _sut.Delete(personId);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<PersonResponse>().Subject;

        model.Id.Should().Be(personId);

        _personQueryServiceMock.Verify(x => x.GetByIdAsync(personId), Times.Once);
    }

    [Fact]
    public async Task Delete_Get_WithInvalidId_ShouldRedirectToIndex()
    {
        // Arrange
        var personId = Guid.NewGuid();

        _personQueryServiceMock
            .Setup(x => x.GetByIdAsync(personId))
            .ReturnsAsync((PersonResponse?)null);

        // Act
        var result = await _sut.Delete(personId);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(PersonsController.Index));
    }

    [Fact]
    public async Task Delete_Post_InvalidModel_ShouldReturnViewWithErrors()
    {
        // Arrange
        var person = _fixture.Create<PersonResponse>();
        _sut.ModelState.AddModelError("Id", "Invalid Id");

        // Act
        var result = await _sut.Delete(person);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(person);

        _personCommandServiceMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Delete_Post_ValidModelButPersonNotFound_ShouldRedirectToIndex()
    {
        // Arrange
        var person = _fixture.Create<PersonResponse>();

        _personQueryServiceMock
            .Setup(x => x.GetByIdAsync(person.Id))
            .ReturnsAsync((PersonResponse?)null);

        // Act
        var result = await _sut.Delete(person);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(PersonsController.Index));

        _personCommandServiceMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Delete_Post_ValidModel_ShouldDeleteAndRedirect()
    {
        // Arrange
        var person = _fixture.Create<PersonResponse>();

        _personQueryServiceMock.Setup(x => x.GetByIdAsync(person.Id)).ReturnsAsync(person);

        _personCommandServiceMock.Setup(x => x.DeleteAsync(person.Id)).ReturnsAsync(true);

        // Act
        var result = await _sut.Delete(person);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(PersonsController.Index));

        _personCommandServiceMock.Verify(x => x.DeleteAsync(person.Id), Times.Once);
    }

    #endregion

    #region File Download Tests

    [Fact]
    public async Task DownloadPersonsCsv_ShouldReturnCsvFile()
    {
        // Arrange
        var csvBytes = _fixture.Create<byte[]>();

        _personQueryServiceMock.Setup(x => x.GetPersonsCsvAsync()).ReturnsAsync(csvBytes);

        // Act
        var result = await _sut.DownloadPersonsCsv();

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.FileContents.Should().BeEquivalentTo(csvBytes);
        fileResult.ContentType.Should().Be("text/csv");
        fileResult.FileDownloadName.Should().Be("persons.csv");

        _personQueryServiceMock.Verify(x => x.GetPersonsCsvAsync(), Times.Once);
    }

    [Fact]
    public async Task DownloadPersonsExcel_ShouldReturnExcelFile()
    {
        // Arrange
        var excelBytes = _fixture.Create<byte[]>();

        _personQueryServiceMock.Setup(x => x.GetPersonsExcelAsync()).ReturnsAsync(excelBytes);

        // Act
        var result = await _sut.DownloadPersonsExcel();

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.FileContents.Should().BeEquivalentTo(excelBytes);
        fileResult
            .ContentType.Should()
            .Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        fileResult.FileDownloadName.Should().MatchRegex(@"Persons_\d{14}\.xlsx");

        _personQueryServiceMock.Verify(x => x.GetPersonsExcelAsync(), Times.Once);
    }

    #endregion
}
