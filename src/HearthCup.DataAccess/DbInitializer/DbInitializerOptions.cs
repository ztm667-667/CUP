namespace HearthCup.DataAccess.DbInitializer;

/// <summary>Everything the initialiser needs, filled in by the web host at start-up.</summary>
public sealed class DbInitializerOptions
{
    public required string ContentRootPath { get; init; }

    /// <summary>Login name of the single manager account, seeded once into the database.</summary>
    public string AdminEmail { get; init; } = "";

    /// <summary>Only used when the manager row does not exist yet; stored as a hash.</summary>
    public string AdminPassword { get; init; } = "";

    /// <summary>Development also writes the credential file the browser checks read.</summary>
    public bool IsDevelopment { get; init; }
}
