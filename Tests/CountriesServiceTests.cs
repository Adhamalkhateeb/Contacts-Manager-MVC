using System.Runtime.CompilerServices;
using System.Security.AccessControl;
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
        _countriesService = new CountriesService(false);
    }

    #region  AddCountry

    [Fact]
    public void Add_CountryIsNull_ArgumentNullException()
    {
        //Arrange
        CountryAddRequest request = null!;

        //Assert
        Assert.Throws<ArgumentNullException>(
            () => _countriesService.Add(request)
        );

    }

    [Fact]
    public void Add_NameIsNull_ArgumentNullException()
    {
        var request = new CountryAddRequest { Name = null };

        Assert.Throws<ArgumentNullException>(() =>
            _countriesService.Add(request));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Add_NameIsEmptyOrWhitespace_ArgumentException(string name)
    {
        var request = new CountryAddRequest { Name = name };

        Assert.Throws<ArgumentException>(() =>
            _countriesService.Add(request));
    }

    [Fact]
    public void Add_CountryNameIsDuplicate_ArgumentException()
    {
        CountryAddRequest request1 = new CountryAddRequest { Name = "USA" };
        CountryAddRequest request2 = new CountryAddRequest { Name = "USA" };


        _countriesService.Add(request1);

        Assert.Throws<ArgumentException>(() =>
            _countriesService.Add(request2)
        );
    }


    [Fact]
    public void Add_ProperCountryDetails_NewCountryObject()
    {
        CountryAddRequest request = new CountryAddRequest { Name = "EGYPT" };

        var response = _countriesService.Add(request);
        var countries = _countriesService.GetAll();

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("EGYPT", response.Name);
        Assert.Contains(response, countries);
    }

    #endregion

    #region  GetAllCountries

    [Fact]
    public void GetAll_NoCountriesExist_EmptyList()
    {
        var actualResponse = _countriesService.GetAll();

        Assert.Empty(actualResponse);
    }

    [Fact]
    public void GetAll_AddMoreThanOneCountry_NewAddedCountries()
    {
        var countryRequestList = new List<CountryAddRequest>
        {
            new CountryAddRequest {Name = "USA"},
            new CountryAddRequest {Name = "EGYPT"}
        };

        var countriesResponseList = new List<CountryResponse>();

        foreach (var request in countryRequestList)
            countriesResponseList.Add(_countriesService.Add(request));

        var actualCountryResponseList = _countriesService.GetAll();

        foreach (var expected in countriesResponseList)
        {
            Assert.Contains(expected, actualCountryResponseList);
        }
    }

    #endregion

    #region GetById

    [Fact]
    public void GetById_IdIsNull_Null()
    {
        Guid? id = null;

        CountryResponse? response = _countriesService.GetById(id);

        Assert.Null(response);
    }

    [Fact]
    public void GetById_ProperId_CountryResponse()
    {
        var addRequest = new CountryAddRequest() { Name = "EGYPT" };
        var countryResponseFromAdd = _countriesService.Add(addRequest);

        var countryResponseFromGetById =
         _countriesService.GetById(countryResponseFromAdd.Id);

        Assert.Equal(countryResponseFromAdd, countryResponseFromGetById);
    }


    #endregion
}
