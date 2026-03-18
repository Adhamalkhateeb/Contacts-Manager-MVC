using FluentValidation;

namespace ContactsManager.Application.Features.Persons.Queries.GetPersonById;

public sealed class GetPersonByIdQueryValidator : AbstractValidator<GetPersonByIdQuery>
{
    public GetPersonByIdQueryValidator()
    {
        RuleFor(x => x.personId).NotEmpty();
    }
}
