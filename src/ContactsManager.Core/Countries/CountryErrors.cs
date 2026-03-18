using ContactsManager.Domain.Common.Results;

namespace ContactsManager.Domain.Countries;

public static class CountryErrors
{
    public static Error NameRequired =>
        Error.Validation("Country_Name_Required", "Country Name is required");

    public static Error CountryNameTooLong =>
        Error.Validation("Country_Name_TooLong", "Country Name cannot exceed 100 characters");
}
