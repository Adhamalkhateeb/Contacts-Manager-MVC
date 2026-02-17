using System;
using System.ComponentModel.DataAnnotations;
using Entities;
using ServiceContracts.Enums;

namespace ServiceContracts.DTO;



/// <summary>
/// DTO contains new person data to be updated to
/// </summary>
public class PersonUpdateRequest
{
    [Required(ErrorMessage = "Person Id is required")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Person Name can't be Blank")]
    public string? Name { get; set; }
    public Gender? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Email can't be blank")]
    [EmailAddress(ErrorMessage = "Email should be in valid format")]
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool ReceiveNewsLetter { get; set; }
    public Guid CountryId { get; set; }

    /// <summary>
    /// Converts Current PersonUpdateRequest object to a  Person Object
    /// </summary>
    /// <returns>Person Object</returns>
    public Person ToPerson()
    {
        return new Person
        {
            Id = Id,
            Name = Name,
            Gender = Gender.ToString(),
            DateOfBirth = DateOfBirth,
            Email = Email,
            Address = Address,
            ReceiveNewsLetter = ReceiveNewsLetter,
            CountryId = CountryId
        };
    }
}
