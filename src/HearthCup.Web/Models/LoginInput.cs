using System.ComponentModel.DataAnnotations;

namespace HearthCup.Web.Models;

public sealed class LoginInput
{
    [Required, StringLength(80)]
    public string Username { get; set; } = "";

    [Required, StringLength(200)]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}
