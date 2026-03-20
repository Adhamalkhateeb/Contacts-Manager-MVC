namespace ContactsManager.Application.Features.Persons.Common;

public interface IPersonCommand
{
    string Name { get; }
    string Email { get; }
    string? Address { get; }
    DateTime? DateOfBirth { get; }
    Guid CountryId { get; }
}
