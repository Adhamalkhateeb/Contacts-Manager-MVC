using AutoFixture;
using Entities;
using FluentAssertions;
using Moq;
using RepositoriesContract;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;

namespace ServicesTests.CountryTests;

public class CountryCommandServiceTests
{
    private readonly ICountryCommandService _countryCommandService;

    private readonly Mock<ICountriesRepository> _countriesRepositoryMock;
    private readonly IFixture _fixture;

    public CountryCommandServiceTests()
    {
        _fixture = new Fixture();

        _countriesRepositoryMock = new Mock<ICountriesRepository>();

        _countryCommandService = new CountryCommandService(_countriesRepositoryMock.Object);
    }

    #region  AddCountry

    [Fact]
    public async Task AddAsync_CountryIsNull_ArgumentNullException()
    {
        var act = async () => await _countryCommandService.AddAsync(null);

        await act.Should().ThrowAsync<ArgumentNullException>();

        _countriesRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Country>()), Times.Never);
        _countriesRepositoryMock.Verify(x => x.GetByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_NameIsNull_ArgumentNullException()
    {
        var request = _fixture
            .Build<CountryAddRequest>()
            .With(p => p.Name, null as string)
            .Create();

        var act = async () => await _countryCommandService.AddAsync(request);

        await act.Should().ThrowAsync<ArgumentNullException>();

        _countriesRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Country>()), Times.Never);
        _countriesRepositoryMock.Verify(x => x.GetByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddAsync_NameIsEmptyOrWhitespace_ArgumentException(string name)
    {
        var request = _fixture.Build<CountryAddRequest>().With(p => p.Name, name).Create();

        var act = async () => await _countryCommandService.AddAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();

        _countriesRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Country>()), Times.Never);
        _countriesRepositoryMock.Verify(x => x.GetByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_CountryNameIsDuplicate_ArgumentException()
    {
        var request = _fixture.Build<CountryAddRequest>().With(p => p.Name, "Egypt").Create();

        var country = request.ToCountry();

        _countriesRepositoryMock
            .Setup(x => x.GetByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(country);

        var act = async () => await _countryCommandService.AddAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();

        _countriesRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Country>()), Times.Never);
        _countriesRepositoryMock.Verify(x => x.GetByNameAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ValidCountry_NewCountryResponseObject()
    {
        CountryAddRequest request = _fixture
            .Build<CountryAddRequest>()
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

        var response = await _countryCommandService.AddAsync(request);

        response.Should().NotBeNull();
        response.Id.Should().NotBe(Guid.Empty);

        capturedCountry.Should().NotBeNull();
        capturedCountry.Name.Should().Be(request.Name);

        _countriesRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Country>()), Times.Once);
        _countriesRepositoryMock.Verify(x => x.GetByNameAsync(It.IsAny<string>()), Times.Once);
    }

    #endregion
}
