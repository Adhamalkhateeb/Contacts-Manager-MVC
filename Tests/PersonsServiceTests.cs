using System;
using System.ComponentModel.DataAnnotations;
using Entities;
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
    private readonly ITestOutputHelper _testOutputHelper;

    public PersonsServiceTests(ITestOutputHelper testOutputHelper)
    {
        _countriesService = new CountriesService(false);
        _sut = new PersonsService(_countriesService, false);
        _testOutputHelper = testOutputHelper;
    }

    #region Helpers

    private List<PersonResponse> SeedPersons()
    {
        var country = _countriesService.Add(new CountryAddRequest
        {
            Name = "EGYPT"
        });

        var requests = new List<PersonAddRequest>
        {
            new()
            {
                Name = "Adham",
                Email = "adham@gmail.com",
                DateOfBirth = new DateTime(2006, 4, 15),
                Address = "Street 1",
                ReceiveNewsLetters = true,
                Gender = Gender.Male,
                CountryId = country.Id,
            },
            new()
            {
                Name = "Menna",
                Email = "menna@gmail.com",
                DateOfBirth = new DateTime(2003, 3, 1),
                Address = "Street 2",
                ReceiveNewsLetters = true,
                Gender = Gender.Female,
                CountryId = country.Id
            },
            new()
            {
                Name = "Merna",
                Email = "merna@gmail.com",
                DateOfBirth = new DateTime(2003, 3, 1),
                Address = "Street 3",
                ReceiveNewsLetters = true,
                Gender = Gender.Female,
                CountryId = country.Id
            },
            new()
            {
                Name = "Fawzy",
                Email = "fawzy@gmail.com",
                DateOfBirth = new DateTime(1974, 10, 15),
                Address = "Street 4",
                ReceiveNewsLetters = false,
                Gender = Gender.Male,
                CountryId = country.Id
            }
        };

        return requests.Select(r => _sut.Add(r)).ToList();
    }

    #endregion

    #region  Add

    [Fact]
    public void Add_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Add(null));
    }

    [Fact]
    public void Add_NullName_ThrowsArgumentException()
    {
        var request = new PersonAddRequest { Name = null };

        Assert.Throws<ArgumentException>(() => _sut.Add(request));
    }

    [Fact]
    public void Add_ValidRequest_ReturnsPersonResponse()
    {
        var country = _countriesService.Add(new CountryAddRequest { Name = "EGYPT" });

        var request = new PersonAddRequest
        {
            Name = "Adham",
            Email = "adham@gmail.com",
            Gender = Gender.Male,
            CountryId = country.Id
        };

        var response = _sut.Add(request);

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("Adham", response.Name);
    }

    #endregion

    #region  GetPersonById

    [Fact]
    public void GetById_NullId_ReturnsNull()
    {
        var result = _sut.GetById(null);

        Assert.Null(result);
    }


    [Fact]
    public void GetById_NotFound_ReturnsNull()
    {
        var result = _sut.GetById(Guid.NewGuid());

        Assert.Null(result);
    }


    [Fact]
    public void GetById_ValidId_ReturnsPerson()
    {
        var persons = SeedPersons();
        var expected = persons.First();

        var result = _sut.GetById(expected.Id);

        Assert.Equal(expected, result);
    }

    #endregion

    #region  GetAllPersons

    [Fact]
    public void GetAll_Empty_ReturnsEmptyList()
    {
        var result = _sut.GetAll();

        Assert.Empty(result);
    }

    [Fact]
    public void GetAll_AfterSeeding_ReturnsAllPersons()
    {
        var expected = SeedPersons();

        var result = _sut.GetAll();

        Assert.Equal(expected.Count, result.Count);

        foreach (var person in expected)
        {
            Assert.Contains(person, result);
        }
    }

    #endregion

    #region GetFilteredPersons

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetFiltered_SearchValueNullOrEmpty_ReturnsAll(string? searchValue)
    {
        var expected = SeedPersons();

        var result = _sut.GetFiltered(nameof(PersonResponse.Name), searchValue);

        Assert.Equal(expected.Count, result.Count);
    }




    [Theory]
    [InlineData(nameof(PersonResponse.Name), "me")]
    [InlineData(nameof(PersonResponse.Email), "gmail")]
    [InlineData(nameof(PersonResponse.DateOfBirth), "15 Apr 2006")]
    [InlineData(nameof(PersonResponse.Gender), "Male")]
    [InlineData(nameof(PersonResponse.Country), "EGYPT")]
    public void GetFiltered_ValidFields_ReturnsExpected(string field, string searchValue)
    {
        // Arrange
        var persons = SeedPersons();

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
        var result = _sut.GetFiltered(field, searchValue);

        // Assert
        Assert.Equal(expected.Count, result.Count);
        foreach (var person in expected)
            Assert.Contains(person, result);
    }


    [Fact]
    public void GetFiltered_NoMatch_ReturnsEmpty()
    {
        SeedPersons();

        var result = _sut.GetFiltered(nameof(PersonResponse.Name), "NotExist");

        Assert.Empty(result);
    }

    [Fact]
    public void GetFiltered_InvalidSearchBy_ReturnsAll()
    {
        var expected = SeedPersons();

        var result = _sut.GetFiltered("InvalidField", "Adham");

        Assert.Equal(expected.Count, result.Count);
    }


    [Fact]
    public void GetFiltered_IsCaseInsensitive()
    {
        var persons = SeedPersons();

        var expected = persons
            .Where(p => p.Name!.Equals("Adham", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var result = _sut.GetFiltered(nameof(PersonResponse.Name), "adham");

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
    public void GetSorted_InvalidOrderBy_ReturnsOriginalList()
    {
        var persons = SeedPersons();

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
    public void GetSorted_Dynamic_AllFieldsAscendingAndDescending(string orderBy)
    {
        var persons = SeedPersons();

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
    public void Update_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Update(null));
    }

    [Fact]
    public void Update_InvalidId_ThrowsArgumentException()
    {
        var request = new PersonUpdateRequest
        {
            Id = Guid.Empty
        };

        Assert.Throws<ArgumentException>(() => _sut.Update(request));
    }


    [Fact]
    public void Update_PersonNotFound_ThrowsArgumentException()
    {
        var request = new PersonUpdateRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test"
        };

        Assert.Throws<ArgumentException>(() => _sut.Update(request));
    }

    [Fact]
    public void Update_ValidRequest_UpdatesPersonSuccessfully()
    {

        var persons = SeedPersons();
        var existing = persons.First();

        var updateRequest = new PersonUpdateRequest
        {
            Id = existing.Id,
            Name = "Updated Name",
            Email = "updated@email.com",
            Address = "New Address",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(2000, 1, 1),
            CountryId = existing.CountryId!.Value,
            ReceiveNewsLetters = false
        };


        var updated = _sut.Update(updateRequest);
        var personAfterUpdated = _sut.GetById(updateRequest.Id);
        var allPersons = _sut.GetAll();


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
    public void Delete_NullPersonId_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Delete(null));
    }

    [Fact]
    public void Delete_EmptyId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _sut.Delete(Guid.Empty));
    }

    [Fact]
    public void Delete_PersonNotFound_ThrowArgumentException()
    {
        SeedPersons();

        var result = _sut.Delete(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public void Delete_ValidPersonId_ReturnTrue()
    {
        var persons = SeedPersons();
        var personToDelete = persons.First();

        var result = _sut.Delete(personToDelete.Id);
        var allPersonsAfterDelete = _sut.GetAll();

        Assert.True(result);
        Assert.Equal(persons.Count - 1, allPersonsAfterDelete.Count);
    }

    #endregion

}




