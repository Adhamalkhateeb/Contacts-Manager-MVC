using System;

namespace Entities;

/// <summary>
/// Person Domain Model class
/// </summary>
public class Person
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public Guid CountryId { get; set; }
    public bool ReceiveNewsLetter { get; set; }

}
