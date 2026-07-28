using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.Contracts.Requests.Identity;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(
        50,
        MinimumLength = 3,
        ErrorMessage = "Username must be between 3 and 50 characters"
    )]
    [RegularExpression(
        @"^[a-zA-Z0-9_]+$",
        ErrorMessage = "Username can only contain letters, numbers, and underscores"
    )]
    [Display(Name = "Username")]
    [Remote(
        action: "IsUserNameAvailable",
        controller: "Account",
        ErrorMessage = "This username is already taken."
    )]
    public string UserName { get; set; } = default!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    [Display(Name = "Email address")]
    [Remote(
        action: "IsEmailAvailable",
        controller: "Account",
        ErrorMessage = "This email is already in use."
    )]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^\+?0[125]\d{9}$", ErrorMessage = "Invalid phone number format")]
    [Display(Name = "Phone number")]
    [DataType(DataType.PhoneNumber)]
    public string PhoneNumber { get; set; } = default!;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = default!;

    [Required(ErrorMessage = "Confirm Password is required")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = default!;

    // public Role Role { get; set; } = Role.User;
}
