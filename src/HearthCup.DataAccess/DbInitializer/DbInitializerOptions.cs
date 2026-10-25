namespace HearthCup.DataAccess.DbInitializer;

/// <summary>Where the web host keeps the seed catalogue and the local backups. Registered by Program.cs.</summary>
public sealed class DbInitializerOptions
{
    public required string ContentRootPath { get; init; }
}
