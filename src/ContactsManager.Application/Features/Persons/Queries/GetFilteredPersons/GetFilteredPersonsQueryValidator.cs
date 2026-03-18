using ContactsManager.Application.Features.Persons.DTOs;
using FluentValidation;

namespace ContactsManager.Application.Features.Persons.Queries.GetFilteredPersons;

public sealed class GetFilteredPersonsQueryValidator : AbstractValidator<GetFilteredPersonsQuery>
{
    private static readonly string[] AllowedFields =
    [
        nameof(PersonDto.Name),
        nameof(PersonDto.Email),
        nameof(PersonDto.Address),
        nameof(PersonDto.Gender),
        nameof(PersonDto.DateOfBirth),
        nameof(PersonDto.CountryId),
    ];

    public GetFilteredPersonsQueryValidator()
    {
        RuleFor(x => x.SearchBy)
            .NotEmpty()
            .Must(field => AllowedFields.Contains(field))
            .WithMessage($"SearchBy must be one of: {string.Join(", ", AllowedFields)}");
    }
}
