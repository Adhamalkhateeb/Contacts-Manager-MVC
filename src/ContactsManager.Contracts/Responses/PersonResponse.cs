namespace ContactsManager.Contracts.Responses;

/// <summary>
/// DTO for returning Person details
/// </summary>
public class PersonResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string Gender { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public double? Age { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool ReceiveNewsLetters { get; set; }
    public Guid? CountryId { get; set; }
    public string? Country { get; set; }
}
