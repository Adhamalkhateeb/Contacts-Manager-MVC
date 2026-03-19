using ContactsManager.Contracts.Responses;

namespace ContactsManager.Web.Models;

/// <summary>
/// Model class used to represent the data required for displaying a list of persons, along with search and sorting options.
/// </summary>
public record PersonsListViewModel
{
    public IEnumerable<PersonResponse> Persons { get; set; } = new List<PersonResponse>();
    public string? SearchBy { get; set; }
    public string? SearchValue { get; set; }
    public string OrderBy { get; set; } = nameof(PersonResponse.Name);
    public string SortOrder { get; set; } = "ASC";
    public Dictionary<string, string> SearchFields { get; set; } = new();
}
