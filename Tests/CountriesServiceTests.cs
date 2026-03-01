using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using AutoFixture;
using Entities;
using EntityFrameworkCoreMock;
using FluentAssertions;
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
        var act = async () => await _countriesService.AddAsync(null);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AddAsync_NameIsNull_ArgumentNullException()
    {
        var request = _fixture.Build<CountryAddRequest>()
            .With(p => p.Name, null as string)
            .Create();

        var act = async () => await _countriesService.AddAsync(request);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddAsync_NameIsEmptyOrWhitespace_ArgumentException(string name)
    {
        var request = _fixture.Build<CountryAddRequest>()
             .With(p => p.Name, name)
             .Create();

        var act = async () => await _countriesService.AddAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
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

        var act = async () => await _countriesService.AddAsync(request2);

        await act.Should().ThrowAsync<ArgumentException>();
    }


    [Fact]
    public async Task AddAsync_ProperCountryDetails_NewCountryObject()
    {
        CountryAddRequest request = _fixture.Build<CountryAddRequest>()
                                        .With(p => p.Name, "EGYPT")
                                        .Create();

        var response = await _countriesService.AddAsync(request);
        var countries = await _countriesService.GetAllAsync();

        response.Id.Should().NotBe(Guid.Empty);
        response.Name.Should().Be("EGYPT");
        countries.Should().Contain(response);
    }

    #endregion

    #region  GetAllCountries

    [Fact]
    public async Task GetAllAsync_NoCountriesExist_EmptyList()
    {
        var result = await _countriesService.GetAllAsync();
        result.Should().BeEmpty();
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

        actualCountryResponseList.Should().BeEquivalentTo(countriesResponseList);
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetByIdAsync_IdIsNull_Null()
    {
        var result = await _countriesService.GetByIdAsync(null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetById_ProperId_CountryResponse()
    {
        var addRequest = _fixture.Create<CountryAddRequest>();

        var expected = await _countriesService.AddAsync(addRequest);
        var result =
         await _countriesService.GetByIdAsync(expected.Id);

        result.Should().Be(expected);
    }


    #endregion
}
