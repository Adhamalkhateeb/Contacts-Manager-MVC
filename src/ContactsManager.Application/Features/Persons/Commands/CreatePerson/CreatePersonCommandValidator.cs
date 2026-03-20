using ContactsManager.Application.Features.Persons.Common;
using FluentValidation;

namespace ContactsManager.Application.Features.Persons.Commands.CreatePerson;

public sealed class CreatePersonCommandValidator : PersonCommandValidatorBase<CreatePersonCommand>
{
    public CreatePersonCommandValidator()
    {
        AddCommonRules();
    }
}
