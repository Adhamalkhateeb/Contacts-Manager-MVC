using System.Linq.Expressions;
using AutoFixture;
using Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RepositoriesContract;
using Serilog;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using Xunit.Abstractions;

namespace ServicesTests.PersonsTests;

public class PersonQueryServiceTests
{
    private readonly IPersonQueryService _sut;
    private readonly Mock<ICountryQueryService> _countriesServiceMock;
    private readonly Mock<IPersonsRepository> _personsRepositoryMock;
    private readonly IFixture _fixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public PersonQueryServiceTests(ITestOutputHelper testOutputHelper)
    {
        _fixture = new Fixture();

        _personsRepositoryMock = new Mock<IPersonsRepository>();
        _countriesServiceMock = new Mock<ICountryQueryService>();
        var loggerMock = new Mock<ILogger<PersonQueryService>>();
        var diagnosticContextMock = new Mock<IDiagnosticContext>();

        _sut = new PersonQueryService(
            _personsRepositoryMock.Object,
            _countriesServiceMock.Object,
            loggerMock.Object,
            diagnosticContextMock.Object
        );

        _testOutputHelper = testOutputHelper;
    }

    #region Helpers

    private List<Person> SeedPersons()
    {
        var egypt = _fixture
            .Build<Country>()
            .With(c => c.Name, "Egypt")
            .Without(c => c.Persons)
            .Create();

        var usa = _fixture
            .Build<Country>()
            .With(c => c.Name, "USA")
            .Without(c => c.Persons)
            .Create();

        var persons = new List<Person>
        {
            _fixture
                .Build<Person>()
                .With(p => p.Email, "adham@gmail.com")
                .With(p => p.Name, "Adham")
                .With(p => p.Country, egypt)
                .With(p => p.CountryId, egypt.Id)
                .Create(),
            _fixture
                .Build<Person>()
                .With(p => p.Email, "ziad@gmail.com")
                .With(p => p.Name, "Ziad")
                .With(p => p.Country, egypt)
                .With(p => p.CountryId, egypt.Id)
                .Create(),
            _fixture
                .Build<Person>()
                .With(p => p.Email, "ramdan@gmail.com")
                .With(p => p.Name, "Ramdan")
                .With(p => p.Country, egypt)
                .With(p => p.CountryId, egypt.Id)
                .Create(),
            _fixture
                .Build<Person>()
                .With(p => p.Email, "ahmed@gmail.com")
                .With(p => p.Name, "Ahmed")
                .With(p => p.Country, usa)
                .With(p => p.CountryId, usa.Id)
                .Create(),
        };

        return persons;
    }

    #endregion


    #region  GetPersonById

    [Fact]
    public async Task GetByIdAsync_NullId_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        _personsRepositoryMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync((Person?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_ShouldReturnPerson()
    {
        var person = _fixture
            .Build<Person>()
            .With(p => p.Email, "adham@gmail.com")
            .With(p => p.Country, null as Country)
            .Create();

        var expected = person.ToPersonResponse();

        _personsRepositoryMock.Setup(x => x.GetById(person.Id)).ReturnsAsync(person);

        var result = await _sut.GetByIdAsync(person.Id);

        result.Should().BeEquivalentTo(expected);
    }

    #endregion

    #region  GetAllPersons

    [Fact]
    public async Task GetAllAsync_Empty_ShouldReturnEmpty()
    {
        var persons = Enumerable.Empty<Person>();

        _personsRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(persons);

        var result = await _sut.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenRepositoryHasData_ReturnsAllPersons()
    {
        var persons = SeedPersons();

        var expected = persons.Select(p => p.ToPersonResponse()).ToList();

        _personsRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(persons);

        var result = await _sut.GetAllAsync();

        result.Should().BeEquivalentTo(expected);
    }

    #endregion

    #region GetFilteredPersons

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task GetFiltered_SearchValueNullOrWhiteSpace_ReturnsAll(string? searchValue)
    {
        var persons = SeedPersons();

        _personsRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(persons);

        var result = await _sut.GetFilteredAsync(nameof(PersonResponse.Name), searchValue);

        result.Should().BeEquivalentTo(persons.Select(p => p.ToPersonResponse()));

        _personsRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        _personsRepositoryMock.Verify(
            x => x.GetFilteredAsync(It.IsAny<Expression<Func<Person, bool>>>()),
            Times.Never
        );
    }

    [Fact]
    public async Task GetFiltered_NoMatch_ReturnsEmpty()
    {
        _personsRepositoryMock
            .Setup(x => x.GetFilteredAsync(It.IsAny<Expression<Func<Person, bool>>>()))
            .ReturnsAsync(new List<Person>());

        var result = await _sut.GetFilteredAsync(nameof(PersonResponse.Name), "NotExist");

        result.Should().BeEmpty();

        _personsRepositoryMock.Verify(
            x => x.GetFilteredAsync(It.IsAny<Expression<Func<Person, bool>>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetFiltered_InvalidSearchBy_ReturnsAll()
    {
        var persons = SeedPersons();
        var expected = persons.Select(p => p.ToPersonResponse());

        _personsRepositoryMock
            .Setup(x => x.GetFilteredAsync(It.IsAny<Expression<Func<Person, bool>>>()))
            .ReturnsAsync(persons);

        var result = await _sut.GetFilteredAsync("InvalidField", "Adham");

        result.Should().BeEquivalentTo(expected);

        _personsRepositoryMock.Verify(
            x => x.GetFilteredAsync(It.IsAny<Expression<Func<Person, bool>>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetFiltered_IsCaseInsensitive()
    {
        var persons = SeedPersons();

        _personsRepositoryMock
            .Setup(x => x.GetFilteredAsync(It.IsAny<Expression<Func<Person, bool>>>()))
            .ReturnsAsync(
                (Expression<Func<Person, bool>> predicate) =>
                {
                    var compiled = predicate.Compile();
                    return persons.Where(compiled).ToList();
                }
            );

        var expected = persons
            .Select(p => p.ToPersonResponse())
            .Where(p => p.Name!.Equals("Adham", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var result = await _sut.GetFilteredAsync(nameof(PersonResponse.Name), "adham");

        result
            .Should()
            .OnlyContain(p => p.Name!.Contains("Adham", StringComparison.OrdinalIgnoreCase));
        result.Should().BeEquivalentTo(expected);

        _personsRepositoryMock.Verify(
            x => x.GetFilteredAsync(It.IsAny<Expression<Func<Person, bool>>>()),
            Times.Once
        );
    }

    [Theory]
    [InlineData(nameof(PersonResponse.Name), "me")]
    [InlineData(nameof(PersonResponse.Email), "gmail")]
    [InlineData(nameof(PersonResponse.DateOfBirth), "15 Apr 2006")]
    [InlineData(nameof(PersonResponse.Gender), "Male")]
    [InlineData(nameof(PersonResponse.CountryId), "EGYPT")]
    public async Task GetFiltered_ValidFields_ReturnsExpected(string field, string searchValue)
    {
        // Arrange
        var persons = SeedPersons();

        var responses = persons.Select(p => p.ToPersonResponse()).ToList();

        _personsRepositoryMock
            .Setup(p => p.GetFilteredAsync(It.IsAny<Expression<Func<Person, bool>>>()))
            .ReturnsAsync(
                (Expression<Func<Person, bool>> predicate) =>
                {
                    var compiled = predicate.Compile();
                    return persons.Where(compiled).ToList();
                }
            );

        // Act

        var result = await _sut.GetFilteredAsync(field, searchValue);

        var expected = field switch
        {
            nameof(PersonResponse.Name) => responses.Where(p =>
                p.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
            ),

            nameof(PersonResponse.Email) => responses.Where(p =>
                p.Email!.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
            ),

            nameof(PersonResponse.DateOfBirth) => responses.Where(p =>
                p.DateOfBirth.HasValue
                && p.DateOfBirth.Value.ToString("dd MMMM yyyy")
                    .Contains(searchValue, StringComparison.OrdinalIgnoreCase)
            ),

            nameof(PersonResponse.Gender) => responses.Where(p =>
                !string.IsNullOrEmpty(p.Gender)
                && p.Gender.Equals(searchValue, StringComparison.OrdinalIgnoreCase)
            ),

            nameof(PersonResponse.CountryId) => responses.Where(p =>
                !string.IsNullOrEmpty(p.Country)
                && p.Country.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
            ),

            _ => Enumerable.Empty<PersonResponse>(),
        };

        // Assert

        expected = expected.ToList();
        result.Should().BeEquivalentTo(expected);

        _personsRepositoryMock.Verify(
            x => x.GetFilteredAsync(It.IsAny<Expression<Func<Person, bool>>>()),
            Times.Once
        );
    }

    #endregion

    #region GetSortedPersons

    [Fact]
    public void GetSorted_EmptyList_ReturnsEmptyList()
    {
        var persons = new List<PersonResponse>();

        var result = _sut.GetSorted(persons, nameof(PersonResponse.Name), SortOrder.ASC);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetSorted_InvalidOrderBy_ReturnsOriginalList()
    {
        var persons = SeedPersons().Select(p => p.ToPersonResponse()).ToList();

        var result = _sut.GetSorted(persons, "invalid", SortOrder.ASC);

        result.Should().BeEquivalentTo(persons, o => o.WithStrictOrdering());
    }

    [Theory]
    [InlineData(nameof(PersonResponse.Name))]
    [InlineData(nameof(PersonResponse.Email))]
    [InlineData(nameof(PersonResponse.DateOfBirth))]
    [InlineData(nameof(PersonResponse.Gender))]
    [InlineData(nameof(PersonResponse.Country))]
    [InlineData(nameof(PersonResponse.Age))]
    [InlineData(nameof(PersonResponse.ReceiveNewsLetters))]
    [InlineData(nameof(PersonResponse.Address))]
    public async Task GetSorted_Dynamic_AllFieldsAscendingAndDescending(string orderBy)
    {
        var persons = SeedPersons().Select(p => p.ToPersonResponse()).ToList();

        object? GetPropValue(PersonResponse p)
        {
            var prop = typeof(PersonResponse).GetProperty(orderBy);
            if (prop == null)
                return null;
            var value = prop.GetValue(p, null);
            if (value is string s)
                return s.ToLower();
            return value;
        }

        var ascResult = _sut.GetSorted(persons, orderBy, SortOrder.ASC);
        var expectedAsc = persons.OrderBy(GetPropValue).ToList();

        ascResult.Should().BeEquivalentTo(expectedAsc, o => o.WithStrictOrdering());

        var descResult = _sut.GetSorted(persons, orderBy, SortOrder.DESC);
        var expectedDesc = persons.OrderByDescending(GetPropValue).ToList();

        descResult.Should().BeEquivalentTo(expectedDesc, o => o.WithStrictOrdering());
    }

    #endregion
}
