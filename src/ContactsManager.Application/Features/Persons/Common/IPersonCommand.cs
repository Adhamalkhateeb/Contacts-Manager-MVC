namespace ContactsManager.Application.Features.Persons.Helpers;

public interface IPersonCommand
{
    string Name { get; }
    string Email { get; }
    string? Address { get; }
    DateTime? DateOfBirth { get; }
    Guid CountryId { get; }
}
