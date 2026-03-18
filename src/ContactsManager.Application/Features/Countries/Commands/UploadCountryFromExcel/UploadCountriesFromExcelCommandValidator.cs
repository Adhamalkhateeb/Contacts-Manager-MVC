using FluentValidation;

namespace ContactsManager.Application.Features.Countries.Commands.UploadCountryFromExcel;

public class UploadCountriesFromExcelCommandValidator
    : AbstractValidator<UploadCountriesFromExcelCommand>
{
    public UploadCountriesFromExcelCommandValidator()
    {
        RuleFor(x => x.file)
            .NotNull()
            .WithMessage("File is required")
            .Must(f => f.Length > 0)
            .WithMessage("File cannot be empty")
            .Must(f => f.FileName.EndsWith(".xlsx"))
            .WithMessage("Only Excel files are allowed");
    }
}
