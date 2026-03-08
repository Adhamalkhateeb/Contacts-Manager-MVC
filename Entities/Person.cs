using System;
using System.ComponentModel.DataAnnotations;

namespace Entities;

/// <summary>
/// Person Domain Model class
/// </summary>
public record Person
{
    public Guid Id { get; set; }
    public string? Name { get; set; }

    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public Guid CountryId { get; set; }
    public bool ReceiveNewsLetters { get; set; }
    public virtual Country? Country { get; set; } = null!;
    public string? Tin { get; set; }
}
