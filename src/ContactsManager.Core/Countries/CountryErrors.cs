using ContactsManager.Domain.Common.Results;

namespace ContactsManager.Domain.Countries;

public static class CountryErrors
{
    public static Error NameRequired => Error.Validation("Name", "Country Name is required");

    public static Error CountryNameTooLong =>
        Error.Validation("Name", "Country Name cannot exceed 100 characters");
}
