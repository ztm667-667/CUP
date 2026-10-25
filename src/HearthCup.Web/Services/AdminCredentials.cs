using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace HearthCup.Web.Services;

/// <summary>
/// The local manager account. In development a random password is generated once and hashed; only the
/// hash is verified at sign-in. Production refuses to start without Admin:PasswordHash configured.
/// </summary>
public sealed class AdminCredentials
{
    public const string UserName = "manager";

    private readonly string hash;
    private readonly PasswordHasher<string> hasher = new();

    public AdminCredentials(IWebHostEnvironment env, IConfiguration config)
    {
        var configured = config["Admin:PasswordHash"];

        if (!string.IsNullOrWhiteSpace(configured))
        {
            hash = configured;
            return;
        }

        if (!env.IsDevelopment())
        {
            throw new InvalidOperationException("Configure Admin:PasswordHash before production use.");
        }

        var directory = Path.Combine(env.ContentRootPath, ".local");
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, "admin.json");

        if (File.Exists(path))
        {
            hash = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))!["hash"];
            return;
        }

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
