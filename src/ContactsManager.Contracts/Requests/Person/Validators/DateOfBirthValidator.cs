using System.ComponentModel.DataAnnotations;

namespace ContactsManager.Contracts.Requests.Person;

public sealed class DateOfBirthValidator
{
    public static ValidationResult? ValidateDateOfBirth(
        DateTime? dateOfBirth,
        ValidationContext context
    )
    {
        if (dateOfBirth.HasValue && dateOfBirth.Value.Date > DateTime.UtcNow.Date)
        {
            return new ValidationResult(
                "Date of birth cannot be in the future",
                new[] { nameof(DateOfBirth) }
            );
        }

        return ValidationResult.Success;
    }
}
