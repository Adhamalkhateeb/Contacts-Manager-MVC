using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services;

public class CountriesService : ICountriesService
{
    private readonly List<Country> _countries;

    public CountriesService(bool initialize = true)
    {
        _countries = [];
        if (initialize)
        {
            _countries.AddRange(new List<Country>{
                new Country
                {
                    Id = Guid.Parse("2f18a149-4122-4b65-8987-69f04bd2b758"),
                    Name = "Egypt"
                },
                new Country
                {
                    Id = Guid.Parse("dbbbcca0-f997-4720-b8fb-37ec2dc71f2e"),
                    Name = "USA"
                },
                new Country
                {
                    Id = Guid.Parse("efd85b43-e69b-4c39-92e9-6843f692fe3a"),
                    Name = "Canada"
                },
                new Country
                {
                    Id = Guid.Parse("59fb67c6-36ef-49e1-963c-bb07606e8b8b"),
                    Name = "UK"
                },
                new Country
                {
                    Id = Guid.Parse("eeafd16e-a9b5-4aab-9036-eb80ca1e2146"),
                    Name = "India"
                }
            });
        }
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
