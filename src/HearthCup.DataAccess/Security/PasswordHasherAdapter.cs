using HearthCup.Models;
using Microsoft.AspNetCore.Identity;

namespace HearthCup.DataAccess.Security;

/// <summary>
/// Wraps the framework's PBKDF2 hasher behind the port declared in the models tier, so the business
/// tier can hash and verify without taking a dependency on ASP.NET Core Identity.
/// </summary>
public sealed class PasswordHasherAdapter : IPasswordHasher
{
    private const string Salt = "hearthcup";

    private readonly PasswordHasher<string> inner = new();

    public string Hash(string password) => inner.HashPassword(Salt, password);

    public bool Verify(string passwordHash, string password) =>
        inner.VerifyHashedPassword(Salt, passwordHash, password) != PasswordVerificationResult.Failed;
}
