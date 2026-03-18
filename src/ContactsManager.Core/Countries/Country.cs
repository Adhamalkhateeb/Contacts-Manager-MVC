using ContactsManager.Domain.Common;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Persons;

namespace ContactsManager.Domain.Countries;

/// <summary>
/// Domain Model for Country
/// </summary>
public sealed class Country : AuditableEntity
{
    public string Name { get; private set; } = default!;

    private Country() { }

    private Country(Guid id, string name)
        : base(id)
    {
        Name = name;
    }

    public static Result<Country> Create(Guid id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return CountryErrors.NameRequired;

        if (name.Trim().Length > 100)
            return CountryErrors.CountryNameTooLong;

        return new Country(id, name);
    }
}
