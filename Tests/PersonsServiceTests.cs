using System;
using System.ComponentModel.DataAnnotations;
using AutoFixture;
using Entities;
using EntityFrameworkCoreMock;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using OfficeOpenXml.Drawing.Chart;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using Xunit.Abstractions;

namespace ContactsManager.Tests;

public class PersonsServiceTests
{
    private readonly IPersonsService _sut;
    private readonly ICountriesService _countriesService;
    private readonly IFixture _fixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public PersonsServiceTests(ITestOutputHelper testOutputHelper)
    {
        _fixture = new Fixture();

        var contextOptions = new DbContextOptionsBuilder<AppDbContext>().Options;
        var contextMock = new DbContextMock<AppDbContext>(contextOptions);

        var countriesInitialData = new List<Country>();
        var personsInitialData = new List<Person>();

        contextMock.CreateDbSetMock(x => x.Countries, countriesInitialData);
        contextMock.CreateDbSetMock(x => x.Persons, personsInitialData);

        _countriesService = new CountriesService(contextMock.Object);
        _sut = new PersonsService(contextMock.Object, _countriesService);

        _testOutputHelper = testOutputHelper;
    }

    #region Helpers

    private async Task<List<PersonResponse>> SeedPersons()
    {
        var country1 = await _countriesService.AddAsync(_fixture.Create<CountryAddRequest>());
        var country2 = await _countriesService.AddAsync(_fixture.Create<CountryAddRequest>());

        var requests = new List<PersonAddRequest>
        {
            _fixture.Build<PersonAddRequest>()
                .With(p => p.Email, "adham@gmail.com")
                .With(p => p.Name, "Adham")
                .With(p => p.CountryId, country1.Id)
                .Create(),

            _fixture.Build<PersonAddRequest>()
                .With(p => p.Email, "ziad@gmail.com")
                .With(p => p.Name, "Ziad")
                .With(p => p.CountryId, country1.Id)
                .Create(),

            _fixture.Build<PersonAddRequest>()
                .With(p => p.Email, "ramdan@gmail.com")
                .With(p => p.Name, "Ramdan")
                .With(p => p.CountryId, country2.Id)
                .Create(),

            _fixture.Build<PersonAddRequest>()
                .With(p => p.Email, "ahmed@gmail.com")
                .With(p => p.Name, "Ahmed")
                .With(p => p.CountryId, country2.Id)
                .Create()

        };

        var results = new List<PersonResponse>();
        foreach (var request in requests)
        {
            results.Add(await _sut.AddAsync(request));
        }
        return results;
    }

    #endregion

    #region  Add

    [Fact]
    public async Task AddAsync_NullRequest_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _sut.AddAsync(null));
    }

    [Fact]
    public async Task AddAsync_NullName_ThrowsArgumentException()
    {
        var request = _fixture.Build<PersonAddRequest>()
                    .With(p => p.Name, null as string)
                    .Create();

        await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.AddAsync(request));
    }

    [Fact]
    public async Task AddAsync_InvalidCountry_ThrowsArgumentException()
    {

        var request = _fixture.Build<PersonAddRequest>()
                     .With(p => p.CountryId, Guid.Empty)
                     .Create();

        await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.AddAsync(request));
    }

    [Fact]
    public async Task AddAsync_ValidRequest_ReturnsPersonResponse()
    {

        var country = await _countriesService.AddAsync(_fixture.Create<CountryAddRequest>());

        var request = _fixture.Build<PersonAddRequest>()
            .With(p => p.Name, "Adham")
            .With(p => p.Email, "adham@gmail.com")
            .With(p => p.CountryId, country.Id)
            .Create();


        var response = await _sut.AddAsync(request);
        var allPersons = await _sut.GetAllAsync();

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("Adham", response.Name);
        Assert.Contains(response, allPersons);
    }

    #endregion

    #region  GetPersonById

    [Fact]
    public async Task GetByIdAsync_NullId_ReturnsNull()
    {
        Assert.Null(await _sut.GetByIdAsync(null));
    }


    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        Assert.Null(await _sut.GetByIdAsync(Guid.NewGuid()));
    }


    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsPerson()
    {
        var country = await _countriesService.AddAsync(_fixture.Create<CountryAddRequest>());
        var request = _fixture.Build<PersonAddRequest>()
            .With(p => p.Email, "adham@gmail.com")
            .With(p => p.CountryId, country.Id)
            .Create();

        var expected = await _sut.AddAsync(request);
        var result = await _sut.GetByIdAsync(expected.Id);

        Assert.Equal(expected, result);
    }

    #endregion

    #region  GetAllPersons

    [Fact]
    public async Task GetAllAsync_Empty_ReturnsEmptyList()
    {
        Assert.Empty(await _sut.GetAllAsync());
    }

    [Fact]
    public async Task GetAllAsync_AfterSeeding_ReturnsAllPersons()
    {
        var persons = await SeedPersons();
        var result = await _sut.GetAllAsync();

        Assert.Equal(persons.Count, result.Count);

        foreach (var person in result)
        {
            Assert.Contains(person, persons);
        }
    }

    #endregion

    #region GetFilteredPersons

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetFiltered_SearchValueNullOrEmpty_ReturnsAll(string? searchValue)
    {
        var expected = await SeedPersons();

        var result = _sut.GetFiltered(expected, nameof(PersonResponse.Name), searchValue);

        Assert.Equal(expected.Count, result.Count);
    }




    [Theory]
    [InlineData(nameof(PersonResponse.Name), "me")]
    [InlineData(nameof(PersonResponse.Email), "gmail")]
    [InlineData(nameof(PersonResponse.DateOfBirth), "15 Apr 2006")]
    [InlineData(nameof(PersonResponse.Gender), "Male")]
    [InlineData(nameof(PersonResponse.Country), "EGYPT")]
    public async Task GetFiltered_ValidFields_ReturnsExpected(string field, string searchValue)
    {
        // Arrange
        var persons = await SeedPersons();

        List<PersonResponse> expected = field switch
        {
            nameof(PersonResponse.Name) =>
                persons.Where(p => p.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase)).ToList(),

            nameof(PersonResponse.Email) =>
                persons.Where(p => p.Email!.Contains(searchValue, StringComparison.OrdinalIgnoreCase)).ToList(),

            nameof(PersonResponse.DateOfBirth) =>
                persons.Where(p => p.DateOfBirth.HasValue &&
                                    p.DateOfBirth.Value.ToString("dd MMMM yyyy").Contains(searchValue, StringComparison.OrdinalIgnoreCase))
                                    .ToList(),

            nameof(PersonResponse.Gender) =>
                persons.Where(p => !string.IsNullOrEmpty(p.Gender) &&
                                    p.Gender.Equals(searchValue, StringComparison.OrdinalIgnoreCase))
                                    .ToList(),

            nameof(PersonResponse.Country) =>
                persons.Where(p => !string.IsNullOrEmpty(p.Country) &&
                                    p.Country.Contains(searchValue, StringComparison.OrdinalIgnoreCase))
                                    .ToList(),

            _ => new List<PersonResponse>()
        };

        // Act
        var result = _sut.GetFiltered(persons, field, searchValue);

        // Assert
        Assert.Equal(expected.Count, result.Count);
        foreach (var person in expected)
            Assert.Contains(person, result);
    }


    [Fact]
    public async Task GetFiltered_NoMatch_ReturnsEmpty()
    {
        var persons = await SeedPersons();

        var result = _sut.GetFiltered(persons, nameof(PersonResponse.Name), "NotExist");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFiltered_InvalidSearchBy_ReturnsAll()
    {
        var expected = await SeedPersons();

        var result = _sut.GetFiltered(expected, "InvalidField", "Adham");

        Assert.Equal(expected.Count, result.Count);
    }


    [Fact]
    public async Task GetFiltered_IsCaseInsensitive()
    {
        var persons = await SeedPersons();

        var expected = persons
            .Where(p => p.Name!.Equals("Adham", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var result = _sut.GetFiltered(persons, nameof(PersonResponse.Name), "adham");

        Assert.Equal(expected.Count, result.Count);
        Assert.Contains(expected.First(), result);
    }

    #endregion

    #region GetSortedPersons

    [Fact]
    public void GetSorted_EmptyList_ReturnsEmptyList()
    {
        var persons = new List<PersonResponse>();

        var result = _sut.GetSorted(persons, nameof(PersonResponse.Name), SortOrder.ASC);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSorted_InvalidOrderBy_ReturnsOriginalList()
    {
        var persons = await SeedPersons();

        var result = _sut.GetSorted(persons, "invalid", SortOrder.ASC);

        Assert.Equal(persons.Count, result.Count);
        for (int i = 0; i < persons.Count; i++)
            Assert.Equal(persons[i].Id, result[i].Id);
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
        var persons = await SeedPersons();

        object? GetPropValue(PersonResponse p)
        {
            var prop = typeof(PersonResponse).GetProperty(orderBy);
            if (prop == null) return null;
            var value = prop.GetValue(p, null);
            if (value is string s) return s.ToLower();
            return value;
        }

        var ascResult = _sut.GetSorted(persons, orderBy, SortOrder.ASC);
        var expectedAsc = persons.OrderBy(GetPropValue).ToList();

        Assert.Equal(expectedAsc.Count, ascResult.Count);
        for (int i = 0; i < expectedAsc.Count; i++)
            Assert.Equal(expectedAsc[i].Id, ascResult[i].Id);

        var descResult = _sut.GetSorted(persons, orderBy, SortOrder.DESC);
        var expectedDesc = persons.OrderByDescending(GetPropValue).ToList();

        Assert.Equal(expectedDesc.Count, descResult.Count);
        for (int i = 0; i < expectedDesc.Count; i++)
            Assert.Equal(expectedDesc[i].Id, descResult[i].Id);
    }

    #endregion

    #region  UpdatePerson

    [Fact]
    public async Task UpdateAsync_NullRequest_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _sut.UpdateAsync(null));
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ThrowsArgumentException()
    {
        var request = _fixture.Build<PersonUpdateRequest>()
            .With(p => p.Id, Guid.Empty)
            .Create();

        await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.UpdateAsync(request));
    }


    [Fact]
    public async Task UpdateAsync_PersonNotFound_ThrowsArgumentException()
    {
        var request = _fixture.Create<PersonUpdateRequest>();

        await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.UpdateAsync(request));
    }

    [Fact]
    public async Task UpdateAsync_InvalidCountry_ThrowsArgumentException()
    {
        var persons = await SeedPersons();
        var existing = persons.First();

        var updateRequest = _fixture.Build<PersonUpdateRequest>()
                .With(p => p.Id, existing.Id)
                .With(p => p.Email, "updated@email.com")
                .Create();

        await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.UpdateAsync(updateRequest));
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesPersonSuccessfully()
    {

        var persons = await SeedPersons();
        var existing = persons.First();

        var updateRequest = _fixture.Build<PersonUpdateRequest>()
               .With(p => p.Id, existing.Id)
               .With(p => p.CountryId, existing?.CountryId!.Value)
               .With(p => p.Email, "updated@email.com")
               .With(p => p.ReceiveNewsLetters, false)
               .Create();



        var updated = await _sut.UpdateAsync(updateRequest);
        var personAfterUpdated = await _sut.GetByIdAsync(updateRequest.Id);
        var allPersons = await _sut.GetAllAsync();


        Assert.Equal(updateRequest.Id, updated.Id);
        Assert.Equal(updateRequest.Name, updated.Name);
        Assert.Equal(updateRequest.Email, updated.Email);
        Assert.Equal(updateRequest.Address, updated.Address);
        Assert.Equal(updateRequest.Gender.ToString(), updated.Gender);
        Assert.Equal(updateRequest.DateOfBirth, updated.DateOfBirth);
        Assert.False(updated.ReceiveNewsLetters);


        Assert.Equal(persons.Count, allPersons.Count);
        Assert.Equal(updated, personAfterUpdated);
        Assert.Contains(allPersons, p => p.Id == updated.Id && p.Name == updateRequest.Name);

    }


    #endregion

    #region DeletePerson


    [Fact]
    public async Task DeleteAsync_InvalidId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.DeleteAsync(Guid.Empty));
    }

    [Fact]
    public async Task DeleteAsync_PersonNotFound_ThrowArgumentException()
    {
        await SeedPersons();

        var result = await _sut.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ValidPersonId_ReturnTrue()
    {
        var persons = await SeedPersons();
        var personToDelete = persons.First();

        var result = await _sut.DeleteAsync(personToDelete.Id);
        var allPersonsAfterDelete = await _sut.GetAllAsync();

        Assert.True(result);
        Assert.Equal(persons.Count - 1, allPersonsAfterDelete.Count);
    }

    #endregion

}




