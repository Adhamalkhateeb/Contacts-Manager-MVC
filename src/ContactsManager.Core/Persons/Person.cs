using System.Net.Mail;
using ContactsManager.Domain.Common;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Countries;
using ContactsManager.Domain.Persons.Enums;

namespace ContactsManager.Domain.Persons;

/// <summary>
/// Person Domain Model class
/// </summary>
public sealed class Person : AuditableEntity
{
    public string Name { get; private set; } = default!;
    public Gender Gender { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Address { get; private set; }
    public string Email { get; private set; } = default!;
    public Guid CountryId { get; private set; }
    public bool ReceiveNewsLetters { get; private set; }
    public Country Country { get; private set; } = default!;

    private Person() { }

    private Person(
        Guid id,
        string name,
        Gender gender,
        DateTime? dateOfBirth,
        string email,
        string? address,
        bool receiveNewsLetters,
        Guid countryId
    )
        : base(id)
    {
        Name = name;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        Email = email;
        Address = address;
        ReceiveNewsLetters = receiveNewsLetters;
        CountryId = countryId;
    }

    /// <summary>
    /// Factory method to create a new Person with validations
    /// </summary>
    public static Result<Person> Create(
        Guid id,
        string name,
        Gender gender,
        DateTime? dateOfBirth,
        string email,
        string? address,
        bool receiveNewsLetters,
        Guid countryId
    )
    {
        var validationResult = Validate(name, email, address, dateOfBirth, countryId);
        if (validationResult is not null)
            return validationResult.Value;

        return new Person(
            id,
            name.Trim(),
            gender,
            dateOfBirth,
            email.Trim().ToLowerInvariant(),
            address?.Trim(),
            receiveNewsLetters,
            countryId
        );
    }

    /// <summary>
    /// Updates the person's details in place
    /// </summary>
    public Result<Updated> Update(
        string name,
        Gender gender,
        DateTime? dateOfBirth,
        string email,
        string? address,
        bool receiveNewsLetters,
        Guid countryId
    )
    {
        var validationResult = Validate(name, email, address, dateOfBirth, countryId);
        if (validationResult is not null)
            return validationResult.Value;

        Name = name.Trim();
        Gender = gender;
        DateOfBirth = dateOfBirth;
        Email = email.Trim().ToLowerInvariant();
        Address = address?.Trim();
        ReceiveNewsLetters = receiveNewsLetters;
        CountryId = countryId;

        return Result.Updated;
    }

    private static Error? Validate(
        string name,
        string email,
        string? address,
        DateTime? dateOfBirth,
        Guid countryId
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            return PersonErrors.NameRequired;

        if (name.Length > 100)
            return PersonErrors.NameTooLong;

        if (string.IsNullOrWhiteSpace(email))
            return PersonErrors.EmailRequired;

        if (email.Length > 255)
            return PersonErrors.EmailTooLong;

        try
        {
            _ = new MailAddress(email);
        }
        catch
        {
            return PersonErrors.InvalidEmailFormat;
        }

        if (dateOfBirth > DateTime.UtcNow)
            return PersonErrors.InvalidDateOfBirth;

        if (!string.IsNullOrWhiteSpace(address) && address.Length > 500)
            return PersonErrors.AddressTooLong;

        if (countryId == Guid.Empty)
            return PersonErrors.CountryRequired;

        return null;
    }
}
