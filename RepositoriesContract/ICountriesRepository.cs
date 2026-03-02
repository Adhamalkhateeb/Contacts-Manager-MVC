using Entities;

namespace RepositoriesContract;

/// <summary>
/// Represents data access logic for managing country entity
/// </summary>
public interface ICountriesRepository
{

    /// <summary>
    /// Adds new country object to the data store 
    /// </summary>
    /// <param name="country">Country object to add</param>
    /// <returns>Country object after adding it to the data source</returns>
    Task<Country> AddAsync(Country country);


    /// <summary>
    /// Retrieve all the countries from the data store
    /// </summary>
    /// <returns>all country objects as IEnumerable</returns>
    Task<IEnumerable<Country>> GetAllAsync();


    /// <summary>
    /// Retrieve Single country object based on given id
    /// </summary>
    /// <param name="countryId">country id to search</param>
    /// <returns>Matching country if found or null</returns>
    Task<Country?> GetByIdAsync(Guid countryId);

    /// <summary>
    /// Retrieve Single country object based on country name
    /// </summary>
    /// <param name="countryName">country name to search</param>
    /// <returns>Matching Country if found or null</returns>
    Task<Country?> GetByNameAsync(string countryName);

}
