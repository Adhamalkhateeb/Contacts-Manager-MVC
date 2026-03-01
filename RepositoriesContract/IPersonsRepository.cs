using System.Linq.Expressions;
using Entities;

namespace RepositoriesContract;

/// <summary>
/// Represents data access logic for managing person entity
/// </summary>
public interface IPersonsRepository
{
    /// <summary>
    /// Add a new person object to the data store
    /// </summary>
    /// <param name="person">Person object to add</param>
    /// <returns>Person object after adding it to the data source</returns>
    Task<Person> AddAsync(Person person);


    /// <summary>
    /// Updates person details based on given person Id
    /// </summary>
    /// <param name="person">Person object to update</param>
    /// <returns>The updated person object</returns>
    Task<Person?> UpdateAsync(Person person);


    /// <summary>
    /// Delete person from the data source based on given Id
    /// </summary>
    /// <param name="personId">Id of the person to delete</param>
    /// <returns>True if person deleted successfully, otherwise false</returns>
    Task<int> DeleteAsync(Guid personId);


    /// <summary>
    /// Retrieve Single person object based on given id
    /// </summary>
    /// <param name="countryId">person id to search</param>
    /// <returns>Matching person if found or null</returns>
    Task<Person?> GetById(Guid personId);

    /// <summary>
    /// Retrieve all the persons from the data store
    /// </summary>
    /// <returns>all person objects as IEnumerable</returns>
    Task<IEnumerable<Person>> GetAllAsync();


    /// <summary>
    /// Return all persons based on the given predicate
    /// </summary>
    /// <param name="predicate">LINQ Expression to check</param>
    /// <returns>All matching persons with given condition</returns>
    Task<IEnumerable<Person>> GetFilteredAsync(Expression<Func<Person, bool>> predicate);
}
