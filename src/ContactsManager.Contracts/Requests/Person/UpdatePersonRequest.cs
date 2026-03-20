using System.ComponentModel.DataAnnotations;
using ContactsManager.Contracts.Requests.Person.Enums;

namespace ContactsManager.Contracts.Requests.Person;

/// <summary>
/// Request contract for updating a person
/// </summary>
public class UpdatePersonRequest
{
    [Required(ErrorMessage = "Person ID is required")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Person Name can't be blank")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Please select gender of the person")]
    public Gender? Gender { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is not valid")]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool ReceiveNewsLetters { get; set; }

    [Required(ErrorMessage = "Please select a country")]
    public Guid CountryId { get; set; }
}
