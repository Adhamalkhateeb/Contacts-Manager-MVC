using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using Xunit.Sdk;

namespace Tests;

public class CountriesServiceTests
{
    private readonly ICountriesService _countriesService;

    public CountriesServiceTests()
    {
        _countriesService = new CountriesService(new ContactsManagerDbContext(new DbContextOptionsBuilder<ContactsManagerDbContext>().Options));
    }

    #region  AddCountry

    [Fact]
    public async Task AddAsync_CountryIsNull_ArgumentNullException()
    {
        //Arrange
        CountryAddRequest request = null!;

        //Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _countriesService.AddAsync(request)
        );

    }

    [Fact]
    public async Task AddAsync_NameIsNull_ArgumentNullException()
    {
        var request = new CountryAddRequest { Name = null };

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _countriesService.AddAsync(request));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddAsync_NameIsEmptyOrWhitespace_ArgumentException(string name)
    {
        var request = new CountryAddRequest { Name = name };

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _countriesService.AddAsync(request));
    }

    [Fact]
    public async Task AddAsync_CountryNameIsDuplicate_ArgumentException()
    {
        CountryAddRequest request1 = new CountryAddRequest { Name = "USA" };
        CountryAddRequest request2 = new CountryAddRequest { Name = "USA" };


        await _countriesService.AddAsync(request1);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _countriesService.AddAsync(request2)
        );
    }


    [Fact]
    public async Task AddAsync_ProperCountryDetails_NewCountryObject()
    {
        CountryAddRequest request = new CountryAddRequest { Name = "EGYPT" };

        var response = await _countriesService.AddAsync(request);
        var countries = await _countriesService.GetAllAsync();

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("EGYPT", response.Name);
        Assert.Contains(response, countries);
    }

    #endregion

    #region  GetAllCountries

    [Fact]
    public async Task GetAllAsync_NoCountriesExist_EmptyList()
    {
        var actualResponse = await _countriesService.GetAllAsync();

        Assert.Empty(actualResponse);
    }

    [Fact]
    public async Task GetAllAsync_AddMoreThanOneCountry_NewAddedCountries()
    {
        var countryRequestList = new List<CountryAddRequest>
        {
            new CountryAddRequest {Name = "USA"},
            new CountryAddRequest {Name = "EGYPT"}
        };

        var countriesResponseList = new List<CountryResponse>();

        foreach (var request in countryRequestList)
            countriesResponseList.Add(await _countriesService.AddAsync(request));

        var actualCountryResponseList = await _countriesService.GetAllAsync();

        foreach (var expected in countriesResponseList)
        {
            Assert.Contains(expected, actualCountryResponseList);
        }
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetByIdAsync_IdIsNull_Null()
    {
        Guid? id = null;

        CountryResponse? response = await _countriesService.GetByIdAsync(id);

        Assert.Null(response);
    }

    [Fact]
    public async Task GetById_ProperId_CountryResponse()
    {
        var addRequest = new CountryAddRequest() { Name = "EGYPT" };
        var countryResponseFromAdd = await _countriesService.AddAsync(addRequest);

        var countryResponseFromGetById =
         await _countriesService.GetByIdAsync(countryResponseFromAdd.Id);

        Assert.Equal(countryResponseFromAdd, countryResponseFromGetById);
    }


    #endregion
}
