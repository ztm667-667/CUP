using System.ComponentModel.DataAnnotations;

namespace HearthCup.Models;

/// <summary>Self-service registration. Only buyers can register; the manager comes from configuration.</summary>
public sealed class RegisterInput
{
    [Required(ErrorMessage = "Enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(254)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Choose a six digit password.")]
    [RegularExpression("^[0-9]{6}$", ErrorMessage = "The password must be exactly six digits.")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Repeat the password.")]
    [Compare(nameof(Password), ErrorMessage = "The two passwords do not match.")]
    public string ConfirmPassword { get; set; } = "";
}
