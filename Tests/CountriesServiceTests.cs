using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using AutoFixture;
using Entities;
using EntityFrameworkCoreMock;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using Xunit.Sdk;

namespace Tests;

public class CountriesServiceTests
{
    private readonly ICountriesService _countriesService;
    private readonly IFixture _fixture;

    public CountriesServiceTests()
    {
        _fixture = new Fixture();

        var contextOptions = new DbContextOptionsBuilder<AppDbContext>().Options;
        var contextMock = new DbContextMock<AppDbContext>(contextOptions);

        var countriesInitialData = new List<Country>();

        contextMock.CreateDbSetMock(x => x.Countries, countriesInitialData);

        _countriesService = new CountriesService(contextMock.Object);
    }

    #region  AddCountry

    [Fact]
    public async Task AddAsync_CountryIsNull_ArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _countriesService.AddAsync(null)
        );

    }

    [Fact]
    public async Task AddAsync_NameIsNull_ArgumentNullException()
    {
        var request = _fixture.Build<CountryAddRequest>()
            .With(p => p.Name, null as string)
            .Create();

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _countriesService.AddAsync(request)
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddAsync_NameIsEmptyOrWhitespace_ArgumentException(string name)
    {
        var request = _fixture.Build<CountryAddRequest>()
             .With(p => p.Name, name)
             .Create();

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _countriesService.AddAsync(request));
    }

    [Fact]
    public async Task AddAsync_CountryNameIsDuplicate_ArgumentException()
    {
        CountryAddRequest request1 = _fixture.Build<CountryAddRequest>()
                                        .With(p => p.Name, "Egypt")
                                        .Create();

        CountryAddRequest request2 = _fixture.Build<CountryAddRequest>()
                                        .With(p => p.Name, "Egypt")
                                        .Create();


        await _countriesService.AddAsync(request1);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _countriesService.AddAsync(request2)
        );
    }


    [Fact]
    public async Task AddAsync_ProperCountryDetails_NewCountryObject()
    {
        CountryAddRequest request = _fixture.Build<CountryAddRequest>()
                                        .With(p => p.Name, "EGYPT")
                                        .Create();

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
        Assert.Empty(await _countriesService.GetAllAsync());
    }

    [Fact]
    public async Task GetAllAsync_AddMoreThanOneCountry_NewAddedCountries()
    {
        var countryRequestList = new List<CountryAddRequest>
        {
            _fixture.Build<CountryAddRequest>()
                        .With(p => p.Name, "Egypt")
                        .Create(),

            _fixture.Build<CountryAddRequest>()
                        .With(p => p.Name, "USA")
                        .Create()
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
        Assert.Null(await _countriesService.GetByIdAsync(null));
    }

    [Fact]
    public async Task GetById_ProperId_CountryResponse()
    {
        var addRequest = _fixture.Create<CountryAddRequest>();
        var countryResponseFromAdd = await _countriesService.AddAsync(addRequest);

        var countryResponseFromGetById =
         await _countriesService.GetByIdAsync(countryResponseFromAdd.Id);

        Assert.Equal(countryResponseFromAdd, countryResponseFromGetById);
    }


    #endregion
}
