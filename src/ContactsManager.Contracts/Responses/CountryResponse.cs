namespace ContactsManager.Contracts.Responses;

/// <summary>
/// DTO for returning Country details
/// </summary>
public class CountryResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
}
