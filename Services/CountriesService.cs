using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services;

public class CountriesService : ICountriesService
{
    private readonly List<Country> _countries;

    public CountriesService()
    {
        _countries = [];
    }
    public CountryResponse Add(CountryAddRequest? countryAddRequest)
    {
        ArgumentNullException.ThrowIfNull(countryAddRequest);
        ArgumentException.ThrowIfNullOrWhiteSpace(countryAddRequest.Name);


        if (_countries.Any(c =>
            c.Name!.Equals(countryAddRequest.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Country already exists");
        }

        var country = countryAddRequest.ToCountry();
        country.Id = Guid.NewGuid();

        _countries.Add(country);

        return country.ToCountryResponse();

    }

    public List<CountryResponse> GetAll()
    {
        return _countries.Select(c => c.ToCountryResponse()).ToList();
    }

    public CountryResponse? GetById(Guid? id)
    {
        if (id is null)
            return null;

        return _countries.FirstOrDefault(c => c.Id == id)?.ToCountryResponse();
    }
}
