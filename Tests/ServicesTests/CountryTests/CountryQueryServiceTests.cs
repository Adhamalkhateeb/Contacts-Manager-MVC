using AutoFixture;
using Entities;
using FluentAssertions;
using Moq;
using RepositoriesContract;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;

namespace ServicesTests.CountryTests;

public class CountryQueryServiceTests
{
    private readonly ICountryQueryService _countryQueryService;
    private readonly Mock<ICountriesRepository> _countriesRepositoryMock;
    private readonly IFixture _fixture;

    public CountryQueryServiceTests()
    {
        _fixture = new Fixture();

        _countriesRepositoryMock = new Mock<ICountriesRepository>();

        _countryQueryService = new CountryQueryService(_countriesRepositoryMock.Object);
    }

    #region  GetAllCountries

    [Fact]
    public async Task GetAllAsync_NoCountriesExist_ReturnsEmptyList()
    {
        _countriesRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Country>());

        var result = await _countryQueryService.GetAllAsync();

        result.Should().BeEmpty();

        _countriesRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_AddMoreThanOneCountry_ReturnsNewAddedCountries()
    {
        var countries = new List<Country>
        {
            _fixture.Build<Country>().Without(p => p.Persons).With(p => p.Name, "Egypt").Create(),
            _fixture.Build<Country>().Without(p => p.Persons).With(p => p.Name, "USA").Create(),
        };

        var expected = countries.Select(x => x.ToCountryResponse()).ToList();

        _countriesRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(countries);

        var result = await _countryQueryService.GetAllAsync();

        result.Should().BeEquivalentTo(expected);

        _countriesRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetByIdAsync_IdIsNull_ReturnsNull()
    {
        var result = await _countryQueryService.GetByIdAsync(null);

        result.Should().BeNull();

        _countriesRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetById_ValidId_ReturnsCountryResponse()
    {
        var country = _fixture
            .Build<Country>()
            .Without(c => c.Persons)
            .With(c => c.Name, "Test Country")
            .Create();

        _countriesRepositoryMock.Setup(x => x.GetByIdAsync(country.Id)).ReturnsAsync(country);

        var expected = country.ToCountryResponse();
        var result = await _countryQueryService.GetByIdAsync(country.Id);

        result.Should().BeEquivalentTo(expected);

        _countriesRepositoryMock.Verify(x => x.GetByIdAsync(country.Id), Times.Once);
    }

    #endregion
}
