using System.Data;
using FluentValidation;

namespace ContactsManager.Application.Features.Countries.Queries.GetCountryById;

public sealed class GetCountryByIdQueryValidator : AbstractValidator<GetCountryByIdQuery>
{
    public GetCountryByIdQueryValidator()
    {
        RuleFor(request => request.countryId).NotEmpty().WithMessage("CountryId is required.");
    }
}
