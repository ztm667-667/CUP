using System.ComponentModel.DataAnnotations;
using HearthCup.Models;

namespace HearthCup.Utility.Services;

/// <summary>Registration and sign-in rules. Hashing is delegated to the port, so this stays framework-free.</summary>
public sealed class AccountService(IUserRepository users, IPasswordHasher hasher)
{
    public async Task<User> RegisterAsync(RegisterInput input, CancellationToken ct = default)
    {
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), errors, true))
        {
            throw new ShopRuleException(errors[0].ErrorMessage!);
        }

        var email = Normalise(input.Email);
        if (await users.ExistsAsync(email, ct))
        {
            throw new ShopRuleException("That email address is already registered.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = hasher.Hash(input.Password),
            Role = Roles.Buyer,
            CreatedAt = DateTime.UtcNow
        };

        await users.AddAsync(user, ct);
        return user;
    }

    /// <summary>Returns null for an unknown email and for a wrong password; the caller shows one message.</summary>
    public async Task<User?> SignInAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await users.FindByEmailAsync(Normalise(email), ct);
        if (user is null) return null;

        return hasher.Verify(user.PasswordHash, password) ? user : null;
    }

    private static string Normalise(string email) => email.Trim().ToLowerInvariant();
}
