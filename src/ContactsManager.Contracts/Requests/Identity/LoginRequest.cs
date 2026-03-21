using System.ComponentModel.DataAnnotations;

namespace ContactsManager.Contracts.Requests.Identity;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    [RegularExpression(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "Enter a valid email address"
    )]
    [Display(Name = "Email address")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = default!;

    public bool RememberMe { get; set; } = false;
}
