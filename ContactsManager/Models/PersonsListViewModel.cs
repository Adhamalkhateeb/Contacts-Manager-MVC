using System;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ContactsManager.Models;

public class PersonsListViewModel
{
    public IEnumerable<PersonResponse> Persons { get; set; } = new List<PersonResponse>();

    public string? SearchBy { get; set; }
    public string? SearchValue { get; set; }
    public string OrderBy { get; set; } = nameof(PersonResponse.Name);
    public string SortOrder { get; set; } = "ASC";
    public Dictionary<string, string> SearchFields { get; set; } = new();
}
