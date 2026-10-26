namespace HearthCup.Models;

/// <summary>An order moves from pending to shipped once the manager hands it over.</summary>
public static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Shipped = "Shipped";
}
