using ServiceContracts.DTO;

namespace ServiceContracts;
/// <summary>
/// Represents Business logic for manipulating Country Entity
/// </summary>
public interface ICountriesService
{
    /// <summary>
    /// Add Country Object to the list of countries
    /// </summary>
    /// <param name="countryAddRequest">Country object to add</param>
    /// <returns>Returns the country object after adding it (including new generated id)</returns>
    CountryResponse Add(CountryAddRequest? countryAddRequest);

    /// <summary>
    /// Retrieve all countries 
    /// </summary>
    /// <returns>All Countries as List Of CountryResponse</returns>
    List<CountryResponse> GetAll();

    /// <summary>
    /// Retrieve Country  object based on Id 
    /// </summary>
    /// <param name="Id">Guid Identifier to search</param>
    /// <returns>CountryResponse object</returns>
    CountryResponse? GetById(Guid? Id);
}
