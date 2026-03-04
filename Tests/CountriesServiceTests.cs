using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using AutoFixture;
using Castle.DynamicProxy;
using Entities;
using EntityFrameworkCoreMock;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using RepositoriesContract;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using Xunit.Sdk;

namespace Tests;

public class CountriesServiceTests
{
    private readonly ICountriesService _countriesService;

    private readonly Mock<ICountriesRepository> _countriesRepositoryMock;
    private readonly IFixture _fixture;

    public CountriesServiceTests()
    {
        _fixture = new Fixture();

        _countriesRepositoryMock = new Mock<ICountriesRepository>();

        _countriesService = new CountriesService(_countriesRepositoryMock.Object);
    }

    #region  AddCountry

    [Fact]
    public async Task AddAsync_CountryIsNull_ArgumentNullException()
    {
        var act = async () => await _countriesService.AddAsync(null);

        await act.Should().ThrowAsync<ArgumentNullException>();

        _countriesRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Country>()), Times.Never);
        _countriesRepositoryMock.Verify(x => x.GetByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_NameIsNull_ArgumentNullException()
    {
        var request = _fixture.Build<CountryAddRequest>()
            .With(p => p.Name, null as string)
            .Create();


        var act = async () => await _countriesService.AddAsync(request);

        await act.Should().ThrowAsync<ArgumentNullException>();

        _countriesRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Country>()), Times.Never);
        _countriesRepositoryMock.Verify(x => x.GetByNameAsync(It.IsAny<string>()), Times.Never);
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

        _countriesRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Country>()), Times.Never);
        _countriesRepositoryMock.Verify(x => x.GetByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_CountryNameIsDuplicate_ArgumentException()
    {
        var request = _fixture.Build<CountryAddRequest>()
                            .With(p => p.Name, "Egypt")
                            .Create();

        var country = request.ToCountry();

        _countriesRepositoryMock
                .Setup(x => x.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(country);


        var act = async () => await _countriesService.AddAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();

        _countriesRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Country>()), Times.Never);
        _countriesRepositoryMock.Verify(x => x.GetByNameAsync(It.IsAny<string>()), Times.Once);
    }


    [Fact]
    public async Task AddAsync_ValidCountry_NewCountryResponseObject()
    {
        CountryAddRequest request = _fixture.Build<CountryAddRequest>()
                                        .With(p => p.Name, "EGYPT")
                                        .Create();

        var country = request.ToCountry();

        _countriesRepositoryMock
                .Setup(x => x.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((Country?)null);

        Country? capturedCountry = null;

        _countriesRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Country>()))
                .Callback<Country>(c => capturedCountry = c)
                .ReturnsAsync((Country c) => c);


        var response = await _countriesService.AddAsync(request);

        response.Should().NotBeNull();
        response.Id.Should().NotBe(Guid.Empty);

        capturedCountry.Should().NotBeNull();
        capturedCountry.Name.Should().Be(request.Name);



        _countriesRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Country>()), Times.Once);
        _countriesRepositoryMock.Verify(x => x.GetByNameAsync(It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region  GetAllCountries

    [Fact]
    public async Task GetAllAsync_NoCountriesExist_ReturnsEmptyList()
    {
        _countriesRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Country>());

        var result = await _countriesService.GetAllAsync();

        result.Should().BeEmpty();

        _countriesRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_AddMoreThanOneCountry_ReturnsNewAddedCountries()
    {
        var countries = new List<Country>
        {
            _fixture.Build<Country>()
                        .Without(p => p.Persons)
                        .With(p => p.Name, "Egypt")
                        .Create(),

            _fixture.Build<Country>()
                        .Without(p => p.Persons)
                        .With(p => p.Name, "USA")
                        .Create()
        };

        var expected = countries.Select(x => x.ToCountryResponse()).ToList();


        _countriesRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(countries);

        var result = await _countriesService.GetAllAsync();

        result.Should().BeEquivalentTo(expected);

        _countriesRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetByIdAsync_IdIsNull_ReturnsNull()
    {
        var result = await _countriesService.GetByIdAsync(null);

        result.Should().BeNull();

        _countriesRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetById_ValidId_ReturnsCountryResponse()
    {
        var country = _fixture.Build<Country>()
            .Without(c => c.Persons)
            .With(c => c.Name, "Test Country")
            .Create();

        _countriesRepositoryMock
            .Setup(x => x.GetByIdAsync(country.Id))
            .ReturnsAsync(country);

        var expected = country.ToCountryResponse();
        var result = await _countriesService.GetByIdAsync(country.Id);

        result.Should().BeEquivalentTo(expected);

        _countriesRepositoryMock.Verify(x => x.GetByIdAsync(country.Id), Times.Once);
    }


    #endregion
}
