using ContactsManager.Domain.Common.Results;

namespace ContactsManager.Domain.Persons;

public static class PersonErrors
{
    public static Error NameRequired =>
        Error.Validation("Person_Name_Required", "Person Name is required");

    public static Error NameTooLong =>
        Error.Validation("Person_Name_TooLong", "Person Name cannot exceed 100 characters");

    public static Error EmailRequired =>
        Error.Validation("Person_Email_Required", "Email is required");

    public static Error InvalidEmailFormat =>
        Error.Validation("Person_Email_Invalid", "Email format is invalid");

    public static Error EmailTooLong =>
        Error.Validation("Person_Email_TooLong", "Email cannot exceed 255 characters");

    public static Error CountryRequired =>
        Error.Validation("Person_Country_Required", "Country is required");

    public static Error InvalidDateOfBirth =>
        Error.Validation("Person_DateOfBirth_Invalid", "Date of birth cannot be in the future");
    public static Error AddressTooLong =>
        Error.Validation("Person_Address_TooLong", "Address cannot exceed 500 characters");
}
