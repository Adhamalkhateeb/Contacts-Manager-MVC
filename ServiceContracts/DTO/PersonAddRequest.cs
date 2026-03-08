using System;
using System.ComponentModel.DataAnnotations;
using Entities;
using ServiceContracts.Enums;

namespace ServiceContracts.DTO;

/// <summary>
/// Dto for inserting a new person
/// </summary>
public class PersonAddRequest
{
    [Required(ErrorMessage = "Person Name can't be Blank")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "You must specify your gender")]
    public Gender? Gender { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Email can't be blank")]
    [EmailAddress(ErrorMessage = "Email should be in valid format")]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool ReceiveNewsLetters { get; set; }

    [Required(ErrorMessage = "Please Select a country")]
    public Guid CountryId { get; set; }

    /// <summary>
    /// Converts Current PersonAddRequest object to a new Person Object
    /// </summary>
    /// <returns>Person Object</returns>
    public Person ToPerson()
    {
        return new Person
        {
            Name = Name,
            Gender = Gender.ToString(),
            DateOfBirth = DateOfBirth,
            Email = Email,
            Address = Address,
            ReceiveNewsLetters = ReceiveNewsLetters,
            CountryId = CountryId,
        };
    }
}
