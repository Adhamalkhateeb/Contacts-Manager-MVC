using ContactsManager.Application.Features.Persons.DTOs;
using FluentValidation;

namespace ContactsManager.Application.Features.Persons.Queries.GetSortedPersons;

public sealed class GetSortedPersonsQueryValidator : AbstractValidator<GetSortedPersonsQuery>
{
    private static readonly string[] AllowedFields =
    [
        nameof(PersonDto.Name),
        nameof(PersonDto.Email),
        nameof(PersonDto.DateOfBirth),
        nameof(PersonDto.Gender),
        nameof(PersonDto.CountryId),
    ];

    public GetSortedPersonsQueryValidator()
    {
        RuleFor(x => x.OrderBy)
            .NotEmpty()
            .Must(field => AllowedFields.Contains(field))
            .WithMessage($"OrderBy must be one of: {string.Join(", ", AllowedFields)}");

        RuleFor(x => x.Persons).NotNull();
    }
}
