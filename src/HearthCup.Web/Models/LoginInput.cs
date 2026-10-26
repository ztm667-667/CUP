using System.ComponentModel.DataAnnotations;

namespace HearthCup.Web.Models;

public sealed class LoginInput
{
    [Required(ErrorMessage = "Enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(254)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Enter your password.")]
    [StringLength(200)]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}
