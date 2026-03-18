using ContactsManager.Application.Features.Persons.Helpers;
using FluentValidation;

namespace ContactsManager.Application.Features.Persons.Queries.UpdatePerson;

public sealed class UpdatePersonCommandValidator : PersonCommandValidatorBase<UpdatePersonCommand>
{
    public UpdatePersonCommandValidator()
    {
        RuleFor(request => request.personId).NotEmpty().WithMessage("PersonId is required.");
        AddCommonRules();
    }
}
