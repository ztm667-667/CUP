namespace HearthCup.DataAccess.DbInitializer;

/// <summary>Creates and seeds the coursework database on first run, and re-imports the reviewed catalogue on demand.</summary>
public interface IDbInitializer
{
    Task InitializeAsync();

    Task ImportCatalogueAsync();
}
