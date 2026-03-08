using System;
using System.ComponentModel.DataAnnotations;

namespace Services.Helpers;

public static class ValidationHelper
{
    public static void ValidateModel<T>(T model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            validationResults,
            true);

        if (!isValid)
            throw new ArgumentException(validationResults.FirstOrDefault()?.ErrorMessage);
    }
}

