namespace HearthCup.Models;

/// <summary>A signed-in identity. Buyers register themselves; the single manager is seeded from configuration.</summary>
public sealed class User
{
    public int Id { get; set; }

    /// <summary>Both the login name and the contact address.</summary>
    public string Email { get; set; } = "";

    public string PasswordHash { get; set; } = "";

    public string Role { get; set; } = Roles.Buyer;

    public DateTime CreatedAt { get; set; }
}
