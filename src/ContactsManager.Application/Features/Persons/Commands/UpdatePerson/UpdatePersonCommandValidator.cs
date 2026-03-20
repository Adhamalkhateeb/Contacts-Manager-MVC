using ContactsManager.Application.Features.Persons.Common;
using FluentValidation;

namespace ContactsManager.Application.Features.Persons.Commands.UpdatePerson;

public sealed class UpdatePersonCommandValidator : PersonCommandValidatorBase<UpdatePersonCommand>
{
    public UpdatePersonCommandValidator()
    {
        RuleFor(request => request.PersonId).NotEmpty().WithMessage("PersonId is required.");
        AddCommonRules();
    }
}
