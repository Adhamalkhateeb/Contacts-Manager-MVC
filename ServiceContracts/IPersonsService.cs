using System;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts;

/// <summary>
/// Business Logic for manipulating person entity
/// </summary>
public interface IPersonsService
{
    /// <summary>
    /// Insert new Person into persons list 
    /// </summary>
    /// <param name="personAddRequest">DTO containing person data for inserting</param>
    /// <returns>PersonResponse contain Added person details with Generated Id</returns>
    PersonResponse Add(PersonAddRequest? personAddRequest);

    /// <summary>
    /// Retrieve All Person list 
    /// </summary>
    /// <returns>List of PersonResponse</returns>
    List<PersonResponse> GetAll();

    /// <summary>
    /// Retrieve person based on Id (Guid)
    /// </summary>
    /// <param name="id">Guid Id to search with it</param>
    /// <returns>PersonResponse object contains person Details or null if not found</returns>
    PersonResponse? GetById(Guid? id);

    /// <summary>
    /// Get List of PersonResponse object based on search field and  search value 
    /// </summary>
    /// <param name="searchBy">name of field to search</param>
    /// <param name="searchValue">value to search based on field selected</param>
    /// <returns>all matching persons with the giving criteria</returns>
    List<PersonResponse> GetFiltered(string searchBy, string? searchValue);

    /// <summary>
    /// Sort list of PersonResponse based on specified key (ascending, descending)
    /// </summary>
    /// <param name="persons">list of PersonResponse To sort</param>
    /// <param name="orderBy">the key that sorting will done based on it</param>
    /// <param name="sortOrder">Ascending or Descending</param>
    /// <returns>list of PersonResponse after sorting it</returns>
    List<PersonResponse> GetSorted(List<PersonResponse> persons, string orderBy, SortOrder sortOrder);
}
