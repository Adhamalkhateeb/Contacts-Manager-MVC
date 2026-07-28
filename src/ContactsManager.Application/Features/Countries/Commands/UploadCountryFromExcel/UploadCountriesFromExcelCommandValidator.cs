using FluentValidation;

namespace ContactsManager.Application.Features.Countries.Commands.UploadCountryFromExcel;

public class UploadCountriesFromExcelCommandValidator
    : AbstractValidator<UploadCountriesFromExcelCommand>
{
    private const long MaxFileSizeBytes = 10_485_760;

    public UploadCountriesFromExcelCommandValidator()
    {
        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("File is required")
            .Must(s => s.Length > 0)
            .WithMessage("File cannot be empty");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .Must(name =>
                Path.GetExtension(name).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
            )
            .WithMessage("Only .xlsx files are supported");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithMessage("File cannot be empty")
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage("File size must not exceed 10 MB");
    }
}
