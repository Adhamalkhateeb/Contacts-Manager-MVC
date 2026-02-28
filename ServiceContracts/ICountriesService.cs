using Microsoft.AspNetCore.Http;
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
    Task<CountryResponse> AddAsync(CountryAddRequest? countryAddRequest);

    /// <summary>
    /// Retrieve all countries 
    /// </summary>
    /// <returns>All Countries as List Of CountryResponse</returns>
    Task<List<CountryResponse>> GetAllAsync();

    /// <summary>
    /// Retrieve Country  object based on Id 
    /// </summary>
    /// <param name="Id">Guid Identifier to search</param>
    /// <returns>CountryResponse object</returns>
    Task<CountryResponse?> GetByIdAsync(Guid? Id);

    /// <summary>
    /// Upload Countries from Excel file into database
    /// </summary>
    /// <param name="file"></param>
    /// <returns>Return Number of countries Added</returns>
    Task<int> UploadFromExcelFileAsync(IFormFile file);
}
