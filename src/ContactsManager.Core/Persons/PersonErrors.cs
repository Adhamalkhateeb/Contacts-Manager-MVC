using ContactsManager.Domain.Common.Results;

namespace ContactsManager.Domain.Persons;

public static class PersonErrors
{
    public static Error NameRequired => Error.Validation("Name", "Person Name is required");

    public static Error NameTooLong =>
        Error.Validation("Name", "Person Name cannot exceed 100 characters");

    public static Error EmailRequired => Error.Validation("Email", "Email is required");

    public static Error InvalidEmailFormat => Error.Validation("Email", "Email format is invalid");

    public static Error EmailTooLong =>
        Error.Validation("Email", "Email cannot exceed 255 characters");

    public static Error CountryRequired => Error.Validation("CountryId", "Country is required");

    public static Error InvalidDateOfBirth =>
        Error.Validation("DateOfBirth", "Date of birth cannot be in the future");
    public static Error AddressTooLong =>
        Error.Validation("Address", "Address cannot exceed 500 characters");
}
