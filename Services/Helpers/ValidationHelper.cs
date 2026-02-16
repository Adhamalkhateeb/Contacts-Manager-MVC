using System;
using System.ComponentModel.DataAnnotations;
using ServiceContracts.DTO;

namespace Services.Helpers;

public class ValidationHelper
{
    public static void ValidateModel<T>(T model)
    {
        ArgumentNullException.ThrowIfNull(model);

        List<ValidationResult> validationResults = [];
        ValidationContext validationContext = new ValidationContext(model);
        bool isValid = Validator.TryValidateObject(
            model,
            validationContext,
            validationResults,
            true);


        if (!isValid)
        {
            throw new ArgumentException(validationResults.FirstOrDefault()?.ErrorMessage);
        }
    }
}
