using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace HearthCup.DataAccess.Security;

/// <summary>
/// The local manager account. It sits in the security folder of the data tier because it is the shop's
/// stored credential: development generates a random password once and keeps only its hash, and sign-in
/// verifies that hash. Production refuses to start without Admin:PasswordHash configured.
/// </summary>
public sealed class AdminCredentials
{
    public const string UserName = "manager";

    private readonly string hash;
    private readonly PasswordHasher<string> hasher = new();

    public AdminCredentials(IWebHostEnvironment environment, IConfiguration config)
    {
        var configured = config["Admin:PasswordHash"];

        if (!string.IsNullOrWhiteSpace(configured))
        {
            hash = configured;
            return;
        }

        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException("Configure Admin:PasswordHash before production use.");
        }

        var directory = Path.Combine(environment.ContentRootPath, ".local");
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, "admin.json");

        if (File.Exists(path))
        {
            hash = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))!["hash"];
            return;
        }

        // A random one-off development password; the plain text is written beside the hash only so the
        // developer can sign in locally. It is git-ignored and must never ship with the submission.
        var password = Convert.ToHexString(RandomNumberGenerator.GetBytes(12)) + "a!";
        hash = hasher.HashPassword(UserName, password);

        File.WriteAllText(path, JsonSerializer.Serialize(new Dictionary<string, string> { { "hash", hash } }));
        File.WriteAllText(
            Path.Combine(directory, "admin-login.txt"),
            $"Local coursework manager account\nUsername: {UserName}\nPassword: {password}\nKeep this file private. Not included in source control.");
    }

    public bool Validate(string username, string password) =>
        username == UserName && hasher.VerifyHashedPassword(UserName, hash, password) != PasswordVerificationResult.Failed;
}
